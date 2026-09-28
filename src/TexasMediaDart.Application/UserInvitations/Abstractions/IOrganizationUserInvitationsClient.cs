using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.UserInvitations.Abstractions;

public interface IOrganizationUserInvitationsClient
{
    Task<OrganizationUserDto> AcceptAsync(
        Guid organizationId,
        Guid identityUserId,
        CancellationToken cancellationToken = default);
}