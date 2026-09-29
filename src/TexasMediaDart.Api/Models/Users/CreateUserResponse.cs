using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Api.Models.Users;

public sealed record CreateUserResponse
{
    public required string Status { get; init; }

    public UserDto? User { get; init; }

    public CreateUserInvitationResponse? Invitation { get; init; }
}

public sealed record CreateUserInvitationResponse
{
    public required Guid InvitationId { get; init; }

    public required string Email { get; init; }

    public required Guid OrganizationId { get; init; }

    public required DateTime ExpiresUtc { get; init; }
}