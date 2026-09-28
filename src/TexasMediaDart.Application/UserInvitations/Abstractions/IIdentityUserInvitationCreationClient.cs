using TexasMediaDart.Application.UserInvitations.Models;

namespace TexasMediaDart.Application.UserInvitations.Abstractions;

public interface IIdentityUserInvitationCreationClient
{
    Task<CreateUserInvitationDto> CreateAsync(
        string email,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}