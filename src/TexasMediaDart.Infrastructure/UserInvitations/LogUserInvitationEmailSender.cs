using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TexasMediaDart.Application.UserInvitations.Abstractions;

namespace TexasMediaDart.Infrastructure.UserInvitations;

public sealed class LogUserInvitationEmailSender
    : IUserInvitationEmailSender
{
    private readonly ILogger<LogUserInvitationEmailSender> _logger;
    private readonly string _frontendBaseUrl;

    public LogUserInvitationEmailSender(
        ILogger<LogUserInvitationEmailSender> logger,
        IConfiguration configuration)
    {
        _logger = logger;

        _frontendBaseUrl =
            configuration["Frontend:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException(
                "Frontend base URL is not configured.");
    }

    public Task SendAsync(
        string email,
        string invitationToken,
        DateTime expiresUtc,
        CancellationToken cancellationToken = default)
    {
        var encodedToken =
            Uri.EscapeDataString(invitationToken);

        var invitationUrl =
            $"{_frontendBaseUrl}/#/accept-invitation?token={encodedToken}";

        _logger.LogInformation(
            """
            User invitation email
            Recipient: {Email}
            Expires UTC: {ExpiresUtc}
            Invitation URL: {InvitationUrl}
            """,
            email,
            expiresUtc,
            invitationUrl);

        return Task.CompletedTask;
    }
}