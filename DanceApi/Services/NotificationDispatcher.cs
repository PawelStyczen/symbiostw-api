using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;

namespace DanceApi.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly AppDbContext _context;
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationChannelSender> _senders;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        AppDbContext context,
        IEnumerable<INotificationChannelSender> senders,
        ILogger<NotificationDispatcher> logger)
    {
        _context = context;
        _senders = senders.ToDictionary(sender => sender.Channel);
        _logger = logger;
    }

    public async Task<bool> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var log = new NotificationLog
        {
            Channel = message.Channel,
            Kind = message.Kind,
            Recipient = message.Recipient,
            Subject = message.Subject,
            Provider = ResolveProviderName(message.Channel),
            PlainTextContent = message.PlainTextContent,
            HtmlContent = message.HtmlContent,
            Status = NotificationStatus.Pending,
            RequestedAtUtc = now,
            LastUpdatedAtUtc = now,
            GuestUserId = message.GuestUserId,
            MeetingId = message.MeetingId
        };

        _context.NotificationLogs.Add(log);

        if (!_senders.TryGetValue(message.Channel, out var sender))
        {
            log.Status = NotificationStatus.Skipped;
            log.ErrorMessage = $"No sender registered for channel '{message.Channel}'.";
            log.LastUpdatedAtUtc = DateTime.UtcNow;
            await TrySaveLogAsync(log, cancellationToken);

            _logger.LogWarning(
                "Notification skipped because sender for channel {Channel} is not registered. Recipient: {Recipient}",
                message.Channel,
                message.Recipient);

            return false;
        }

        try
        {
            var result = await sender.SendAsync(message, cancellationToken);

            log.Provider = string.IsNullOrWhiteSpace(result.Provider)
                ? sender.ProviderName
                : result.Provider;
            log.ProviderOperationId = result.ProviderOperationId;
            log.Status = result.Status;
            log.ErrorMessage = result.ErrorMessage;
            log.LastUpdatedAtUtc = DateTime.UtcNow;

            await TrySaveLogAsync(log, cancellationToken);
            return result.Status == NotificationStatus.AcceptedByProvider;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to dispatch notification. Channel: {Channel}, Kind: {Kind}, Recipient: {Recipient}",
                message.Channel,
                message.Kind,
                message.Recipient);

            log.Provider = sender.ProviderName;
            log.Status = NotificationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.LastUpdatedAtUtc = DateTime.UtcNow;

            await TrySaveLogAsync(log, cancellationToken);
            return false;
        }
    }

    private string ResolveProviderName(NotificationChannel channel)
    {
        return _senders.TryGetValue(channel, out var sender)
            ? sender.ProviderName
            : "UnconfiguredProvider";
    }

    private async Task TrySaveLogAsync(NotificationLog log, CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to persist notification log. Channel: {Channel}, Recipient: {Recipient}",
                log.Channel,
                log.Recipient);
        }
    }
}
