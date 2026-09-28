using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Organizations.Abstractions;
using TexasMediaDart.Application.UserInvitations.Abstractions;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
{
    private readonly IIdentityUsersClient _identityUsersClient;
    private readonly IOrganizationUsersClient _organizationUsersClient;
    private readonly IOrganizationsClient _organizationsClient;
    private readonly IIdentityUserInvitationCreationClient
        _identityUserInvitationCreationClient;

    public CreateUserCommandHandler(
        IIdentityUsersClient identityUsersClient,
        IOrganizationUsersClient organizationUsersClient,
        IOrganizationsClient organizationsClient,
        IIdentityUserInvitationCreationClient
            identityUserInvitationCreationClient)
    {
        _identityUsersClient = identityUsersClient;
        _organizationUsersClient = organizationUsersClient;
        _organizationsClient = organizationsClient;
        _identityUserInvitationCreationClient =
            identityUserInvitationCreationClient;
    }

    public async Task<CreateUserResult> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new BadRequestException(
                "Email is required.");
        }

        var email = command.Email.Trim();

        var identityUser =
            await _identityUsersClient.GetByEmailAsync(
                email,
                cancellationToken);

        if (identityUser is null)
        {
            return await CreateInvitationAsync(
                email,
                cancellationToken);
        }

        if (!identityUser.IsActive)
        {
            throw new BadRequestException(
                "The Identity user is inactive.");
        }

        var organizationUser =
            await _organizationUsersClient.CreateAsync(
                identityUser.UserId,
                cancellationToken);

        var user = new UserDto
        {
            OrganizationUserId =
                organizationUser.OrganizationUserId,

            OrganizationId =
                organizationUser.OrganizationId,

            IdentityUserId =
                organizationUser.IdentityUserId,

            Email =
                identityUser.Email,

            IsActive =
                organizationUser.IsActive,

            IsApproved =
                organizationUser.IsApproved,

            IdentityIsActive =
                identityUser.IsActive,

            IsEmailVerified =
                identityUser.IsEmailVerified,

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

        return new CreateUserResult
        {
            Status = "added",
            User = user
        };
    }

    private async Task<CreateUserResult> CreateInvitationAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var organization =
            await _organizationsClient.GetCurrentAsync(
                cancellationToken);

        if (!organization.IsActive)
        {
            throw new BadRequestException(
                "The current organization is inactive.");
        }

        var invitation =
            await _identityUserInvitationCreationClient.CreateAsync(
                email,
                organization.OrganizationId,
                cancellationToken);

        return new CreateUserResult
        {
            Status = "invited",
            Invitation = invitation
        };
    }
}