namespace TexasMediaDart.Application.UserInvitations.Commands.ResendUserInvitation;

public sealed record ResendUserInvitationResult(
    Guid InvitationId,
    string Email,
    Guid OrganizationId,
    DateTime ExpiresUtc);