using TexasMediaDart.Application.UserInvitations.Models;

namespace TexasMediaDart.Application.Users.Models;

public sealed record CreateUserResult
{
    public required string Status { get; init; }

    public UserDto? User { get; init; }

    public CreateUserInvitationDto? Invitation { get; init; }
}