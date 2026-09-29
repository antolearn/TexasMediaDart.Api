using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Organizations.Abstractions;
using TexasMediaDart.Application.UserInvitations.Abstractions;

namespace TexasMediaDart.Application.UserInvitations.Queries.GetPendingUserInvitations;

public sealed class GetPendingUserInvitationsQueryHandler
{
    private readonly IOrganizationsClient _organizationsClient;
    private readonly IIdentityUserInvitationsClient
        _identityUserInvitationsClient;

    public GetPendingUserInvitationsQueryHandler(
        IOrganizationsClient organizationsClient,
        IIdentityUserInvitationsClient identityUserInvitationsClient)
    {
        _organizationsClient = organizationsClient;
        _identityUserInvitationsClient =
            identityUserInvitationsClient;
    }

    public async Task<IReadOnlyList<PendingUserInvitationResult>>
        HandleAsync(
            GetPendingUserInvitationsQuery query,
            CancellationToken cancellationToken = default)
    {
        var organization =
            await _organizationsClient.GetCurrentAsync(
                cancellationToken);

        if (!organization.IsActive)
        {
            throw new BadRequestException(
                "The current organization is inactive.");
        }

        var invitations =
            await _identityUserInvitationsClient.GetPendingAsync(
                organization.OrganizationId,
                cancellationToken);

        return invitations
            .Select(invitation =>
                new PendingUserInvitationResult(
                    invitation.InvitationId,
                    invitation.Email,
                    invitation.OrganizationId,
                    invitation.InvitedByIdentityUserId,
                    invitation.ExpiresUtc,
                    invitation.CreatedUtc))
            .ToList();
    }
}