using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler
{
    private readonly IOrganizationUsersClient _organizationUsersClient;
    private readonly IIdentityUsersClient _identityUsersClient;

    public SearchUsersQueryHandler(
        IOrganizationUsersClient organizationUsersClient,
        IIdentityUsersClient identityUsersClient)
    {
        _organizationUsersClient = organizationUsersClient;
        _identityUsersClient = identityUsersClient;
    }

    public async Task<UserSearchResultDto> HandleAsync(
        SearchUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var organizationResult =
            await _organizationUsersClient.SearchAsync(
                query.IdentityUserId,
                query.IsActive,
                query.IsApproved,
                query.PageNumber,
                query.PageSize,
                cancellationToken);

        if (organizationResult.Items.Count == 0)
        {
            return new UserSearchResultDto
            {
                Items = Array.Empty<UserDto>(),
                TotalCount = organizationResult.TotalCount,
                PageNumber = organizationResult.PageNumber,
                PageSize = organizationResult.PageSize
            };
        }

        var identityUserIds =
            organizationResult.Items
                .Select(user => user.IdentityUserId)
                .Distinct()
                .ToArray();

        var identityUsers =
            await _identityUsersClient.LookupAsync(
                identityUserIds,
                cancellationToken);

        var identityUsersById =
            identityUsers.ToDictionary(
                user => user.UserId);

        var users =
            organizationResult.Items
                .Select(organizationUser =>
                {
                    identityUsersById.TryGetValue(
                        organizationUser.IdentityUserId,
                        out var identityUser);

                    return new UserDto
                    {
                        OrganizationUserId =
                            organizationUser.OrganizationUserId,

                        OrganizationId =
                            organizationUser.OrganizationId,

                        IdentityUserId =
                            organizationUser.IdentityUserId,

                        Email =
                            identityUser?.Email,

                        IsActive =
                            organizationUser.IsActive,

                        IsApproved =
                            organizationUser.IsApproved,

                        IdentityIsActive =
                            identityUser?.IsActive ?? false,

                        IsEmailVerified =
                            identityUser?.IsEmailVerified ?? false,

                        CreatedBy =
                            organizationUser.CreatedBy,

                        CreatedUtc =
                            organizationUser.CreatedUtc,

                        ModifiedBy =
                            organizationUser.ModifiedBy,

                        ModifiedUtc =
                            organizationUser.ModifiedUtc,

                        ApprovedBy =
                            organizationUser.ApprovedBy,

                        ApprovedUtc =
                            organizationUser.ApprovedUtc
                    };
                })
                .ToArray();

        return new UserSearchResultDto
        {
            Items = users,
            TotalCount = organizationResult.TotalCount,
            PageNumber = organizationResult.PageNumber,
            PageSize = organizationResult.PageSize
        };
    }
}