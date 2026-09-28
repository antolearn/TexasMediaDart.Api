namespace TexasMediaDart.Application.UserInvitations.Models;

public sealed record FinalizeInvitationDto(
    Guid InvitationId,
    Guid IdentityUserId,
    string Email,
    Guid OrganizationId,
    DateTime IdentityCreatedUtc,
    DateTime AcceptedUtc);