using TexasMediaDart.Application.UserInvitations.Models;

namespace TexasMediaDart.Application.UserInvitations.Abstractions;

public interface IIdentityUserInvitationsClient
{
    Task<AcceptInvitationIdentityDto> AcceptAsync(
        string token,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken = default);

    Task<FinalizeInvitationDto> FinalizeAsync(
        Guid invitationId,
        Guid identityUserId,
        CancellationToken cancellationToken = default);
}