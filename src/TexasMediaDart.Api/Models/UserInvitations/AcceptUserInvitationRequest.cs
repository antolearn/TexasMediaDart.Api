namespace TexasMediaDart.Api.Models.UserInvitations;

public sealed class AcceptUserInvitationRequest
{
    public string Token { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;
}