using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.Users.Abstractions;

public interface IOrganizationUsersClient
{
    Task<OrganizationUserSearchResultDto> SearchAsync(
        Guid? identityUserId,
        bool? isActive,
        bool? isApproved,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}