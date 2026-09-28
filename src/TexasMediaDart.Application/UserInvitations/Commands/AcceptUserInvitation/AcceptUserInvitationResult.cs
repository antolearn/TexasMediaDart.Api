namespace TexasMediaDart.Application.UserInvitations.Commands.AcceptUserInvitation;

public sealed record AcceptUserInvitationResult(
    Guid InvitationId,
    Guid IdentityUserId,
    long OrganizationUserId,
    Guid OrganizationId,
    string Email,
    bool IsActive,
    bool IsApproved,
    DateTime IdentityCreatedUtc,
    DateTime AcceptedUtc);