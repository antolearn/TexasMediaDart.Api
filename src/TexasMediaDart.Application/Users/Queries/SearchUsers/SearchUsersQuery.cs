namespace TexasMediaDart.Application.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    Guid? IdentityUserId,
    bool? IsActive,
    bool? IsApproved,
    int PageNumber = 1,
    int PageSize = 25);