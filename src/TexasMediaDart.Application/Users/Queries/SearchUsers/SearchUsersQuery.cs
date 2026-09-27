namespace TexasMediaDart.Application.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    Guid? IdentityUserId,
    string? Email,
    bool? IsActive,
    bool? IsApproved,
    string SortBy = "createdUtc",
    string SortDirection = "desc",
    int PageNumber = 1,
    int PageSize = 25);