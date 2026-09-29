using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TexasMediaDart.Application.UserInvitations.Abstractions;

namespace TexasMediaDart.Infrastructure.UserInvitations;

public sealed class AzureCommunicationUserInvitationEmailSender
    : IUserInvitationEmailSender
{
    private readonly EmailClient _emailClient;
    private readonly ILogger<AzureCommunicationUserInvitationEmailSender> _logger;
    private readonly string _frontendBaseUrl;
    private readonly string _senderAddress;
    private readonly string _senderDisplayName;

    public AzureCommunicationUserInvitationEmailSender(
        IConfiguration configuration,
        ILogger<AzureCommunicationUserInvitationEmailSender> logger)
    {
        _logger = logger;

        var connectionString =
            Environment.GetEnvironmentVariable(
                "COMMUNICATION_SERVICES_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "COMMUNICATION_SERVICES_CONNECTION_STRING is not configured.");
        }

        _frontendBaseUrl =
            configuration["Frontend:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException(
                "Frontend:BaseUrl is not configured.");

        _senderAddress =
            configuration["Email:SenderAddress"]
            ?? throw new InvalidOperationException(
                "Email:SenderAddress is not configured.");

        _senderDisplayName =
            configuration["Email:SenderDisplayName"]
            ?? "TexasMediaDart";

        _emailClient = new EmailClient(connectionString);
    }

    public async Task SendAsync(
        string email,
        string invitationToken,
        DateTime expiresUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(invitationToken);

        var encodedToken =
            Uri.EscapeDataString(invitationToken);

        var invitationUrl =
            $"{_frontendBaseUrl}/#/accept-invitation?token={encodedToken}";

        var subject =
            "You're invited to TexasMediaDart";

        var plainTextContent =
            $"""
            You have been invited to TexasMediaDart.

            Accept your invitation:
            {invitationUrl}

            This invitation expires on {expiresUtc:yyyy-MM-dd HH:mm} UTC.

            If you were not expecting this invitation, you can ignore this email.
            """;

        var htmlContent =
            $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; color: #333333;">
                <div style="max-width: 600px; margin: 0 auto; padding: 24px;">
                    <h2>You're invited to TexasMediaDart</h2>

                    <p>
                        You have been invited to join TexasMediaDart.
                    </p>

                    <p style="margin: 32px 0;">
                        <a href="{invitationUrl}"
                           style="
                               background-color: #f57c00;
                               color: #ffffff;
                               padding: 12px 24px;
                               text-decoration: none;
                               border-radius: 4px;
                               display: inline-block;">
                            Accept Invitation
                        </a>
                    </p>

                    <p>
                        This invitation expires on
                        <strong>{expiresUtc:yyyy-MM-dd HH:mm} UTC</strong>.
                    </p>

                    <p>
                        If the button doesn't work, copy and paste this link
                        into your browser:
                    </p>

                    <p>
                        <a href="{invitationUrl}">
                            {invitationUrl}
                        </a>
                    </p>

                    <p>
                        If you were not expecting this invitation,
                        you can ignore this email.
                    </p>

                    <hr />

                    <p style="font-size: 12px; color: #777777;">
                        {_senderDisplayName}
                    </p>
                </div>
            </body>
            </html>
            """;

        var content =
            new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            };

        var recipients =
            new EmailRecipients(
                new[]
                {
                    new EmailAddress(email)
                });

        var message =
            new EmailMessage(
                _senderAddress,
                recipients,
                content);

        try
        {
            await _emailClient.SendAsync(
                WaitUntil.Completed,
                message,
                cancellationToken);

            _logger.LogInformation(
                "User invitation email sent successfully to {Email}.",
                email);
        }
        catch (RequestFailedException exception)
        {
            _logger.LogError(
                exception,
                "Azure Communication Services failed to send invitation email to {Email}.",
                email);

            throw;
        }
    }
}