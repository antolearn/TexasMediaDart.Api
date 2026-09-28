using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.UserInvitations.Abstractions;

namespace TexasMediaDart.Application.UserInvitations.Commands.AcceptUserInvitation;

public sealed class AcceptUserInvitationCommandHandler
{
    private readonly IIdentityUserInvitationsClient
        _identityUserInvitationsClient;

    private readonly IOrganizationUserInvitationsClient
        _organizationUserInvitationsClient;

    public AcceptUserInvitationCommandHandler(
        IIdentityUserInvitationsClient identityUserInvitationsClient,
        IOrganizationUserInvitationsClient organizationUserInvitationsClient)
    {
        _identityUserInvitationsClient =
            identityUserInvitationsClient;

        _organizationUserInvitationsClient =
            organizationUserInvitationsClient;
    }

    public async Task<AcceptUserInvitationResult> HandleAsync(
        AcceptUserInvitationCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new BadRequestException(
                "Invitation token is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new BadRequestException(
                "Password is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ConfirmPassword))
        {
            throw new BadRequestException(
                "Password confirmation is required.");
        }

        if (!string.Equals(
                command.Password,
                command.ConfirmPassword,
                StringComparison.Ordinal))
        {
            throw new BadRequestException(
                "Password and confirmation password must match.");
        }

        var identityResult =
            await _identityUserInvitationsClient.AcceptAsync(
                command.Token.Trim(),
                command.Password,
                command.ConfirmPassword,
                cancellationToken);

        var organizationUser =
            await _organizationUserInvitationsClient.AcceptAsync(
                identityResult.OrganizationId,
                identityResult.IdentityUserId,
                cancellationToken);

        if (organizationUser.OrganizationId !=
            identityResult.OrganizationId)
        {
            throw new ConflictException(
                "The Identity user belongs to a different organization.");
        }

        if (organizationUser.IdentityUserId !=
            identityResult.IdentityUserId)
        {
            throw new ConflictException(
                "The Organization user does not match the accepted identity.");
        }

        var finalized =
            await _identityUserInvitationsClient.FinalizeAsync(
                identityResult.InvitationId,
                identityResult.IdentityUserId,
                cancellationToken);

        return new AcceptUserInvitationResult(
            finalized.InvitationId,
            finalized.IdentityUserId,
            organizationUser.OrganizationUserId,
            organizationUser.OrganizationId,
            finalized.Email,
            organizationUser.IsActive,
            organizationUser.IsApproved,
            finalized.IdentityCreatedUtc,
            finalized.AcceptedUtc);
    }
}