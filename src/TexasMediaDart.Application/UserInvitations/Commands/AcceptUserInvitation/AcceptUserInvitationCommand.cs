namespace TexasMediaDart.Application.UserInvitations.Commands.AcceptUserInvitation;

public sealed record AcceptUserInvitationCommand(
    string Token,
    string Password,
    string ConfirmPassword);