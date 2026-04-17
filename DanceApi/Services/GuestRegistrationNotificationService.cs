using System.Net;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.Extensions.Options;

namespace DanceApi.Services;

public class GuestRegistrationNotificationService : IGuestRegistrationNotificationService
{
    private readonly EmailSettings _emailSettings;
    private readonly INotificationDispatcher _notificationDispatcher;

    public GuestRegistrationNotificationService(
        INotificationDispatcher notificationDispatcher,
        IOptions<EmailSettings> emailSettings)
    {
        _notificationDispatcher = notificationDispatcher;
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendPendingApprovalNotificationAsync(
        GuestUser guestUser,
        Meeting meeting,
        CancellationToken cancellationToken = default)
    {
        var senderName = ResolveSenderName();
        return await SendNotificationAsync(
            guestUser,
            meeting,
            NotificationKind.MeetingRegistrationPendingApproval,
            "Potwierdzenie rejestracji - oczekuje na akceptacje",
            BuildPendingApprovalPlainTextBody(guestUser, meeting, senderName),
            BuildPendingApprovalHtmlBody(guestUser, meeting, senderName),
            cancellationToken);
    }

    public async Task<bool> SendAcceptedNotificationAsync(
        GuestUser guestUser,
        Meeting meeting,
        CancellationToken cancellationToken = default)
    {
        var senderName = ResolveSenderName();
        return await SendNotificationAsync(
            guestUser,
            meeting,
            NotificationKind.MeetingRegistrationAccepted,
            "Twoje zgloszenie zostalo zaakceptowane",
            BuildAcceptedPlainTextBody(guestUser, meeting, senderName),
            BuildAcceptedHtmlBody(guestUser, meeting, senderName),
            cancellationToken);
    }

    public async Task<bool> SendRejectedNotificationAsync(
        GuestUser guestUser,
        Meeting meeting,
        CancellationToken cancellationToken = default)
    {
        var senderName = ResolveSenderName();
        return await SendNotificationAsync(
            guestUser,
            meeting,
            NotificationKind.MeetingRegistrationRejected,
            "Przepraszamy ale mamy juz komplet uczestnikow na to spotkanie",
            BuildRejectedPlainTextBody(guestUser, meeting, senderName),
            BuildRejectedHtmlBody(guestUser, meeting, senderName),
            cancellationToken);
    }

    private Task<bool> SendNotificationAsync(
        GuestUser guestUser,
        Meeting meeting,
        NotificationKind notificationKind,
        string subject,
        string plainText,
        string html,
        CancellationToken cancellationToken)
    {
        var message = new NotificationMessage
        {
            Channel = NotificationChannel.Email,
            Kind = notificationKind,
            Recipient = guestUser.Email ?? string.Empty,
            RecipientDisplayName = $"{guestUser.Name} {guestUser.Surname}".Trim(),
            Subject = subject,
            PlainTextContent = plainText,
            HtmlContent = html,
            GuestUserId = guestUser.Id,
            MeetingId = meeting.Id
        };

        return _notificationDispatcher.SendAsync(message, cancellationToken);
    }

    private static string BuildPendingApprovalPlainTextBody(GuestUser guestUser, Meeting meeting, string senderName)
    {
        var guestName = $"{guestUser.Name} {guestUser.Surname}".Trim();
        var meetingType = meeting.TypeOfMeeting?.Name ?? "spotkanie";
        var meetingLocation = meeting.Location?.Name ?? "lokalizacja zostanie potwierdzona";
        var meetingCity = meeting.Location?.City ?? string.Empty;
        var meetingDate = meeting.Date.ToString("dd.MM.yyyy HH:mm");

        return
$@"Czesc {guestName},

Twoje zgloszenie zostalo zapisane i oczekuje na akceptacje.

Szczegoly zgloszenia:
- Spotkanie: {meetingType}
- Termin: {meetingDate}
- Miejsce: {meetingLocation}{(string.IsNullOrWhiteSpace(meetingCity) ? string.Empty : $", {meetingCity}")}
- Cena: {meeting.Price} PLN

Po akceptacji skontaktujemy sie z Toba lub zaktualizujemy status w systemie.

Pozdrawiamy,
{senderName}";
    }

    private static string BuildPendingApprovalHtmlBody(GuestUser guestUser, Meeting meeting, string senderName)
    {
        var guestName = WebUtility.HtmlEncode($"{guestUser.Name} {guestUser.Surname}".Trim());
        var meetingType = WebUtility.HtmlEncode(meeting.TypeOfMeeting?.Name ?? "spotkanie");
        var meetingLocation = WebUtility.HtmlEncode(meeting.Location?.Name ?? "lokalizacja zostanie potwierdzona");
        var meetingCity = WebUtility.HtmlEncode(meeting.Location?.City ?? string.Empty);
        var meetingDate = WebUtility.HtmlEncode(meeting.Date.ToString("dd.MM.yyyy HH:mm"));
        var locationDisplay = string.IsNullOrWhiteSpace(meetingCity)
            ? meetingLocation
            : $"{meetingLocation}, {meetingCity}";
        var teamName = WebUtility.HtmlEncode(senderName);

        return
$"""
<html>
  <body style="font-family: Arial, sans-serif; color: #1f2937; line-height: 1.6;">
    <p>Czesc {guestName},</p>
    <p>Twoje zgloszenie zostalo zapisane i <strong>oczekuje na akceptacje</strong>.</p>
    <p>Szczegoly zgloszenia:</p>
    <ul>
      <li><strong>Spotkanie:</strong> {meetingType}</li>
      <li><strong>Termin:</strong> {meetingDate}</li>
      <li><strong>Miejsce:</strong> {locationDisplay}</li>
      <li><strong>Cena:</strong> {meeting.Price} PLN</li>
    </ul>
    <p>Po akceptacji skontaktujemy sie z Toba lub zaktualizujemy status w systemie.</p>
    <p>Pozdrawiamy,<br />{teamName}</p>
  </body>
</html>
""";
    }

    private static string BuildAcceptedPlainTextBody(GuestUser guestUser, Meeting meeting, string senderName)
    {
        var guestName = $"{guestUser.Name} {guestUser.Surname}".Trim();
        var meetingType = meeting.TypeOfMeeting?.Name ?? "spotkanie";
        var meetingLocation = meeting.Location?.Name ?? "lokalizacja zostanie potwierdzona";
        var meetingCity = meeting.Location?.City ?? string.Empty;
        var meetingDate = meeting.Date.ToString("dd.MM.yyyy HH:mm");

        return
$@"Czesc {guestName},

Twoje zgloszenie zostalo zaakceptowane.

Szczegoly spotkania:
- Spotkanie: {meetingType}
- Termin: {meetingDate}
- Miejsce: {meetingLocation}{(string.IsNullOrWhiteSpace(meetingCity) ? string.Empty : $", {meetingCity}")}
- Cena: {meeting.Price} PLN

prosimy o dokonanie płatności na:
- numer konta: 36 1140 2004 0000 3002 7666 3697 tytułem: {meetingType} {meetingDate:ddMMyyyy} {guestName}
- przelewem na telefon: 666 617 974 tytułem: {meetingType} {meetingDate:ddMMyyyy} {guestName}
- lub osobiście u organizatora na spotkaniu.

Do zobaczenia,
{senderName}";
    }

    private static string BuildAcceptedHtmlBody(GuestUser guestUser, Meeting meeting, string senderName)
    {
        var guestName = WebUtility.HtmlEncode($"{guestUser.Name} {guestUser.Surname}".Trim());
        var meetingType = WebUtility.HtmlEncode(meeting.TypeOfMeeting?.Name ?? "spotkanie");
        var meetingLocation = WebUtility.HtmlEncode(meeting.Location?.Name ?? "lokalizacja zostanie potwierdzona");
        var meetingCity = WebUtility.HtmlEncode(meeting.Location?.City ?? string.Empty);
        var meetingDate = WebUtility.HtmlEncode(meeting.Date.ToString("dd.MM.yyyy HH:mm"));
        var locationDisplay = string.IsNullOrWhiteSpace(meetingCity)
            ? meetingLocation
            : $"{meetingLocation}, {meetingCity}";
        var teamName = WebUtility.HtmlEncode(senderName);

        return
$"""
<html>
  <body style="font-family: Arial, sans-serif; color: #1f2937; line-height: 1.6;">
    <p>Czesc {guestName},</p>
    <p>Twoje zgloszenie zostalo <strong>zaakceptowane</strong>.</p>
    <p>Szczegoly spotkania:</p>
    <ul>
      <li><strong>Spotkanie:</strong> {meetingType}</li>
      <li><strong>Termin:</strong> {meetingDate}</li>
      <li><strong>Miejsce:</strong> {locationDisplay}</li>
      <li><strong>Cena:</strong> {meeting.Price} PLN</li>
    </ul>

    <p>prosimy o dokonanie płatności na:</p>
    <ul>
      <li>numer konta: <strong>36 1140 2004 0000 3002 7666 3697</strong> tytułem: {meetingType} {meetingDate:ddMMyyyy} {guestName}</li>
      <li>przelewem na telefon: <strong>666 617 974</strong> tytułem: {meetingType} {meetingDate:ddMMyyyy} {guestName}</li>
      <li>lub osobiście u organizatora na spotkaniu.</li>
    </ul>

    <p>Do zobaczenia,<br />{teamName}</p>
  </body>
</html>
""";
    }

    private static string BuildRejectedPlainTextBody(GuestUser guestUser, Meeting meeting, string senderName)
    {
        var guestName = $"{guestUser.Name} {guestUser.Surname}".Trim();
        var meetingType = meeting.TypeOfMeeting?.Name ?? "spotkanie";
        var meetingDate = meeting.Date.ToString("dd.MM.yyyy HH:mm");

        return
$@"Czesc {guestName},

Niestety Twoje zgloszenie na spotkanie nie zostalo zaakceptowane z powodu braku wolnych miejsc.

Szczegoly spotkania:
- Spotkanie: {meetingType}
- Termin: {meetingDate}

W razie pytan skontaktuj sie z nami.
zapraszamy do sledzenia naszej oferty na stronie internetowej i mediach spolecznosciowych, gdzie regularnie informujemy o nowych spotkaniach i wydarzeniach.

Pozdrawiamy,
{senderName}";
    }

    private static string BuildRejectedHtmlBody(GuestUser guestUser, Meeting meeting, string senderName)
    {
        var guestName = WebUtility.HtmlEncode($"{guestUser.Name} {guestUser.Surname}".Trim());
        var meetingType = WebUtility.HtmlEncode(meeting.TypeOfMeeting?.Name ?? "spotkanie");
        var meetingDate = WebUtility.HtmlEncode(meeting.Date.ToString("dd.MM.yyyy HH:mm"));
        var teamName = WebUtility.HtmlEncode(senderName);

        return
$"""
<html>
  <body style="font-family: Arial, sans-serif; color: #1f2937; line-height: 1.6;">
    <p>Czesc {guestName},</p>
    <p>Niestety Twoje zgloszenie na spotkanie nie zostalo zaakceptowane z powodu braku wolnych miejsc.</p>
    <p>Szczegoly spotkania:</p>
    <ul>
      <li><strong>Spotkanie:</strong> {meetingType}</li>
      <li><strong>Termin:</strong> {meetingDate}</li>
    </ul>
    <p>W razie pytan skontaktuj sie z nami.</p>
    <p>zapraszamy do sledzenia naszej oferty na stronie internetowej i mediach spolecznosciowych, gdzie regularnie informujemy o nowych spotkaniach i wydarzeniach.</p>
    <p>Pozdrawiamy,<br />{teamName}</p>
  </body>
</html>
""";
    }

    private string ResolveSenderName()
    {
        return string.IsNullOrWhiteSpace(_emailSettings.SenderName)
            ? "Symbio"
            : _emailSettings.SenderName;
    }
}
