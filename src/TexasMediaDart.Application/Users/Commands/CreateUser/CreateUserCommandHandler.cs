using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
{
    private readonly IIdentityUsersClient _identityUsersClient;
    private readonly IOrganizationUsersClient _organizationUsersClient;

    public CreateUserCommandHandler(
        IIdentityUsersClient identityUsersClient,
        IOrganizationUsersClient organizationUsersClient)
    {
        _identityUsersClient = identityUsersClient;
        _organizationUsersClient = organizationUsersClient;
    }

    public async Task<UserDto> HandleAsync(
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
            throw new NotFoundException(
                "No Identity user exists with the specified email.");
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

        return new UserDto
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
    }
}