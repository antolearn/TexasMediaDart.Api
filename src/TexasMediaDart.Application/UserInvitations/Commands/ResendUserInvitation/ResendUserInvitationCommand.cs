namespace TexasMediaDart.Application.UserInvitations.Commands.ResendUserInvitation;

public sealed record ResendUserInvitationCommand(
    Guid InvitationId);