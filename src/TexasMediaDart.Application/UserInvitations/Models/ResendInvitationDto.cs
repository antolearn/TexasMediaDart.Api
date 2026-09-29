namespace TexasMediaDart.Application.UserInvitations.Models;

public sealed record ResendInvitationDto(
    Guid InvitationId,
    string Email,
    Guid OrganizationId,
    DateTime ExpiresUtc,
    string InvitationToken);