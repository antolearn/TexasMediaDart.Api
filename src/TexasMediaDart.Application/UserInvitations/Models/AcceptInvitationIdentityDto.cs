namespace TexasMediaDart.Application.UserInvitations.Models;

public sealed record AcceptInvitationIdentityDto(
    Guid InvitationId,
    Guid IdentityUserId,
    string Email,
    Guid OrganizationId,
    DateTime IdentityCreatedUtc);