namespace TexasMediaDart.Application.UserInvitations.Queries.GetPendingUserInvitations;

public sealed record PendingUserInvitationResult(
    Guid InvitationId,
    string Email,
    Guid OrganizationId,
    Guid InvitedByIdentityUserId,
    DateTime ExpiresUtc,
    DateTime CreatedUtc);