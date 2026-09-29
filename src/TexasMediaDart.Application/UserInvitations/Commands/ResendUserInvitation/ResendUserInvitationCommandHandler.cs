using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Organizations.Abstractions;
using TexasMediaDart.Application.UserInvitations.Abstractions;

namespace TexasMediaDart.Application.UserInvitations.Commands.ResendUserInvitation;

public sealed class ResendUserInvitationCommandHandler
{
    private readonly IOrganizationsClient _organizationsClient;
    private readonly IIdentityUserInvitationsClient
        _identityUserInvitationsClient;
    private readonly IUserInvitationEmailSender
        _userInvitationEmailSender;

    public ResendUserInvitationCommandHandler(
        IOrganizationsClient organizationsClient,
        IIdentityUserInvitationsClient identityUserInvitationsClient,
        IUserInvitationEmailSender userInvitationEmailSender)
    {
        _organizationsClient = organizationsClient;
        _identityUserInvitationsClient =
            identityUserInvitationsClient;
        _userInvitationEmailSender =
            userInvitationEmailSender;
    }

    public async Task<ResendUserInvitationResult> HandleAsync(
        ResendUserInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.InvitationId == Guid.Empty)
        {
            throw new BadRequestException(
                "Invitation id is required.");
        }

        var organization =
            await _organizationsClient.GetCurrentAsync(
                cancellationToken);

        if (!organization.IsActive)
        {
            throw new BadRequestException(
                "The current organization is inactive.");
        }

        var invitation =
            await _identityUserInvitationsClient.ResendAsync(
                command.InvitationId,
                organization.OrganizationId,
                cancellationToken);

        await _userInvitationEmailSender.SendAsync(
            invitation.Email,
            invitation.InvitationToken,
            invitation.ExpiresUtc,
            cancellationToken);

        return new ResendUserInvitationResult(
            invitation.InvitationId,
            invitation.Email,
            invitation.OrganizationId,
            invitation.ExpiresUtc);
    }
}