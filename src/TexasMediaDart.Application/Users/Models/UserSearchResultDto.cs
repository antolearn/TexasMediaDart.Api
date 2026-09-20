namespace TexasMediaDart.Application.Users.Models;

public sealed class UserSearchResultDto
{
    public IReadOnlyList<UserDto> Items { get; init; } =
        Array.Empty<UserDto>();

    public long TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}