namespace TexasMediaDart.Application.UserInvitations.Models;

public sealed record PendingInvitationDto(
    Guid InvitationId,
    string Email,
    Guid OrganizationId,
    Guid InvitedByIdentityUserId,
    DateTime ExpiresUtc,
    DateTime CreatedUtc);