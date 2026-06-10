using System.Net;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.Extensions.Options;

namespace DanceApi.Services;

public class LeadNotificationService : ILeadNotificationService
{
    private readonly LeadNotificationSettings _settings;
    private readonly INotificationDispatcher _notificationDispatcher;

    public LeadNotificationService(
        IOptions<LeadNotificationSettings> settings,
        INotificationDispatcher notificationDispatcher)
    {
        _settings = settings.Value;
        _notificationDispatcher = notificationDispatcher;
    }

    public Task<bool> SendNewLeadNotificationAsync(
        Lead lead,
        CancellationToken cancellationToken = default)
    {
        var leadName = $"{lead.Name} {lead.Surname}".Trim();
        var subject = $"Nowy lead - {lead.GroupName} - {leadName}";

        var message = new NotificationMessage
        {
            Channel = NotificationChannel.Email,
            Kind = NotificationKind.NewLeadReceived,
            Recipient = _settings.RecipientEmail,
            RecipientDisplayName = "Symbio",
            Subject = subject,
            PlainTextContent = BuildPlainTextBody(lead),
            HtmlContent = BuildHtmlBody(lead),
            LeadId = lead.Id
        };

        return _notificationDispatcher.SendAsync(message, cancellationToken);
    }

    private static string BuildPlainTextBody(Lead lead)
    {
        var leadName = $"{lead.Name} {lead.Surname}".Trim();

        return
$@"Przyszedl nowy lead z formularza.

Dane:
- Lead ID: {lead.Id}
- Imie i nazwisko: {leadName}
- Email: {lead.Email}
- Telefon: {lead.PhoneNumber ?? "brak"}
- Grupa: {lead.GroupName}
- Chce otrzymywac email marketing: {FormatBool(lead.AllowsEmailMarketing)}
- Chce otrzymywac newsletter i SMS marketing: {FormatBool(lead.AllowsNewsletterAndSmsMarketing)}
- Data zgloszenia UTC: {lead.SubmittedAtUtc:yyyy-MM-dd HH:mm:ss}

Dodatkowa wiadomosc:
{(string.IsNullOrWhiteSpace(lead.AdditionalMessage) ? "brak" : lead.AdditionalMessage)}";
    }

    private static string BuildHtmlBody(Lead lead)
    {
        var leadName = WebUtility.HtmlEncode($"{lead.Name} {lead.Surname}".Trim());
        var email = WebUtility.HtmlEncode(lead.Email);
        var phoneNumber = WebUtility.HtmlEncode(lead.PhoneNumber ?? "brak");
        var groupName = WebUtility.HtmlEncode(lead.GroupName);
        var additionalMessage = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(lead.AdditionalMessage)
                ? "brak"
                : lead.AdditionalMessage);

        return
$"""
<html>
  <body style="font-family: Arial, sans-serif; color: #1f2937; line-height: 1.6;">
    <h2>Nowy lead z formularza</h2>
    <table cellpadding="6" cellspacing="0" style="border-collapse: collapse;">
      <tr><td><strong>Lead ID:</strong></td><td>{lead.Id}</td></tr>
      <tr><td><strong>Imie i nazwisko:</strong></td><td>{leadName}</td></tr>
      <tr><td><strong>Email:</strong></td><td>{email}</td></tr>
      <tr><td><strong>Telefon:</strong></td><td>{phoneNumber}</td></tr>
      <tr><td><strong>Grupa:</strong></td><td>{groupName}</td></tr>
      <tr><td><strong>Email marketing:</strong></td><td>{FormatBool(lead.AllowsEmailMarketing)}</td></tr>
      <tr><td><strong>Newsletter i SMS marketing:</strong></td><td>{FormatBool(lead.AllowsNewsletterAndSmsMarketing)}</td></tr>
      <tr><td><strong>Data zgloszenia UTC:</strong></td><td>{lead.SubmittedAtUtc:yyyy-MM-dd HH:mm:ss}</td></tr>
    </table>
    <p><strong>Dodatkowa wiadomosc:</strong></p>
    <p style="white-space: pre-line;">{additionalMessage}</p>
  </body>
</html>
""";
    }

    private static string FormatBool(bool value)
    {
        return value ? "tak" : "nie";
    }
}
