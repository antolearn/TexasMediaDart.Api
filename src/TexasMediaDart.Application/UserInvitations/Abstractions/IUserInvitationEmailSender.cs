namespace TexasMediaDart.Application.UserInvitations.Abstractions;

public interface IUserInvitationEmailSender
{
    Task SendAsync(
        string email,
        string invitationToken,
        DateTime expiresUtc,
        CancellationToken cancellationToken = default);
}