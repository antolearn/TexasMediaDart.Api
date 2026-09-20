namespace TexasMediaDart.Application.Users.Models;

public sealed class IdentityUserDto
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsEmailVerified { get; init; }
}