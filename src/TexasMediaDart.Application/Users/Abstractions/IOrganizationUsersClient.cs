using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.Users.Abstractions;

public interface IOrganizationUsersClient
{
    Task<OrganizationUserSearchResultDto> SearchAsync(
        Guid? identityUserId,
        bool? isActive,
        bool? isApproved,
        string sortBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetCandidateIdentityUserIdsAsync(
        Guid? identityUserId,
        bool? isActive,
        bool? isApproved,
        CancellationToken cancellationToken = default);

    Task<OrganizationUserSearchResultDto> SearchByIdentityIdsAsync(
        OrganizationUserSearchByIdentityIdsRequest request,
        CancellationToken cancellationToken = default);
}