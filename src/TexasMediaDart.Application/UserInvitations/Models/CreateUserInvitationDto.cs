namespace TexasMediaDart.Application.UserInvitations.Models;

public sealed record CreateUserInvitationDto(
    Guid InvitationId,
    string Email,
    Guid OrganizationId,
    DateTime ExpiresUtc,
    string InvitationToken);