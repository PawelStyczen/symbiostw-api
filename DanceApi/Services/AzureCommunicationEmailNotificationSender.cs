using Azure;
using Azure.Communication.Email;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.Extensions.Options;

namespace DanceApi.Services;

public class AzureCommunicationEmailNotificationSender : INotificationChannelSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<AzureCommunicationEmailNotificationSender> _logger;

    public AzureCommunicationEmailNotificationSender(
        IOptions<EmailSettings> emailSettings,
        ILogger<AzureCommunicationEmailNotificationSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public string ProviderName => "AzureCommunicationServicesEmail";

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.Enabled)
        {
            _logger.LogInformation(
                "Email notification skipped because email sending is disabled. Kind: {Kind}, Recipient: {Recipient}",
                message.Kind,
                message.Recipient);

            return new NotificationDispatchResult
            {
                Provider = ProviderName,
                Status = NotificationStatus.Skipped,
                ErrorMessage = "Email sending is disabled."
            };
        }

        if (string.IsNullOrWhiteSpace(message.Recipient))
        {
            _logger.LogWarning(
                "Email notification skipped because recipient email is missing. Kind: {Kind}",
                message.Kind);

            return new NotificationDispatchResult
            {
                Provider = ProviderName,
                Status = NotificationStatus.Skipped,
                ErrorMessage = "Recipient email is missing."
            };
        }

        if (string.IsNullOrWhiteSpace(_emailSettings.ConnectionString) ||
            string.IsNullOrWhiteSpace(_emailSettings.SenderAddress))
        {
            _logger.LogWarning(
                "Email notification skipped because Azure Communication Services settings are incomplete.");

            return new NotificationDispatchResult
            {
                Provider = ProviderName,
                Status = NotificationStatus.Skipped,
                ErrorMessage = "Azure Communication Services settings are incomplete."
            };
        }

        var client = new EmailClient(_emailSettings.ConnectionString);
        var recipients = new EmailRecipients(new[]
        {
            new EmailAddress(message.Recipient, message.RecipientDisplayName)
        });

        var content = new EmailContent(message.Subject ?? "Powiadomienie")
        {
            PlainText = message.PlainTextContent,
            Html = message.HtmlContent
        };

        var emailMessage = new EmailMessage(_emailSettings.SenderAddress, recipients, content)
        {
            UserEngagementTrackingDisabled = true
        };

        var operation = await client.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);

        _logger.LogInformation(
            "Email notification accepted by provider. Kind: {Kind}, Recipient: {Recipient}, OperationId: {OperationId}",
            message.Kind,
            message.Recipient,
            operation.Id);

        return new NotificationDispatchResult
        {
            Provider = ProviderName,
            Status = NotificationStatus.AcceptedByProvider,
            ProviderOperationId = operation.Id
        };
    }
}
