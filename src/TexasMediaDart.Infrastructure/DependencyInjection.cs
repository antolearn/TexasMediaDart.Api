using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TexasMediaDart.Application.Organizations.Abstractions;
using TexasMediaDart.Application.UserInvitations.Abstractions;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Infrastructure.Http;
using TexasMediaDart.Infrastructure.Notifications;
using TexasMediaDart.Infrastructure.Organizations;
using TexasMediaDart.Infrastructure.UserInvitations;
using TexasMediaDart.Infrastructure.Users;

namespace TexasMediaDart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<BearerTokenHandler>();

        //
        // Organization API
        //

        var organizationApiBaseUrl =
            configuration["Services:OrganizationApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Services:OrganizationApi:BaseUrl is not configured.");

        if (!Uri.TryCreate(
                organizationApiBaseUrl,
                UriKind.Absolute,
                out var organizationApiBaseUri))
        {
            throw new InvalidOperationException(
                "Services:OrganizationApi:BaseUrl must be a valid absolute URL.");
        }

        //
        // Identity API
        //

        var identityApiBaseUrl =
            configuration["Services:IdentityApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Services:IdentityApi:BaseUrl is not configured.");

        if (!Uri.TryCreate(
                identityApiBaseUrl,
                UriKind.Absolute,
                out var identityApiBaseUri))
        {
            throw new InvalidOperationException(
                "Services:IdentityApi:BaseUrl must be a valid absolute URL.");
        }

        //
        // Organization users client.
        //
        // Calls execute in the context of the currently
        // authenticated user.
        //

        services
            .AddHttpClient<
                IOrganizationUsersClient,
                OrganizationUsersClient>(
                client =>
                {
                    client.BaseAddress =
                        organizationApiBaseUri;
                })
            .AddHttpMessageHandler<BearerTokenHandler>();

        //
        // Identity users client.
        //
        // Calls execute in the context of the currently
        // authenticated user.
        //

        services
            .AddHttpClient<
                IIdentityUsersClient,
                IdentityUsersClient>(
                client =>
                {
                    client.BaseAddress =
                        identityApiBaseUri;
                })
            .AddHttpMessageHandler<BearerTokenHandler>();

        //
        // Identity user invitations client.
        //
        // Invitation acceptance is anonymous.
        // The Identity service API key is added explicitly
        // inside IdentityUserInvitationsClient.
        //

        services
            .AddHttpClient<
                IIdentityUserInvitationsClient,
                IdentityUserInvitationsClient>(
                client =>
                {
                    client.BaseAddress =
                        identityApiBaseUri;
                });

        //
        // Organization invitation membership acceptance.
        //
        // The Organization service API key is added explicitly
        // inside OrganizationUserInvitationsClient.
        //

        services
            .AddHttpClient<
                IOrganizationUserInvitationsClient,
                OrganizationUserInvitationsClient>(
                client =>
                {
                    client.BaseAddress =
                        organizationApiBaseUri;
                });

        //
        // Current Organization lookup.
        //
        // Calls execute in the context of the currently
        // authenticated user.
        //

        services
            .AddHttpClient<
                IOrganizationsClient,
                OrganizationsClient>(
                client =>
                {
                    client.BaseAddress =
                        organizationApiBaseUri;
                })
            .AddHttpMessageHandler<BearerTokenHandler>();

        //
        // Identity invitation creation.
        //
        // Invitation creation executes in the context
        // of the currently authenticated user.
        //

        services
            .AddHttpClient<
                IIdentityUserInvitationCreationClient,
                IdentityUserInvitationCreationClient>(
                client =>
                {
                    client.BaseAddress =
                        identityApiBaseUri;
                })
            .AddHttpMessageHandler<BearerTokenHandler>();

        //
        // Notification service.
        //
        // User invitation emails are no longer sent directly
        // by the Main API.
        //
        // The Main API submits the invitation notification
        // to TexasMediaDart.Notification.Api.
        //

        services.Configure<NotificationServiceOptions>(
            configuration.GetSection(
                NotificationServiceOptions.SectionName));

        var notificationBaseUrl =
            configuration["NotificationService:BaseUrl"]
            ?? throw new InvalidOperationException(
                "NotificationService:BaseUrl is not configured.");

        if (!Uri.TryCreate(
                notificationBaseUrl,
                UriKind.Absolute,
                out var notificationBaseUri))
        {
            throw new InvalidOperationException(
                "NotificationService:BaseUrl must be a valid absolute URL.");
        }

        services
            .AddHttpClient<
                IUserInvitationEmailSender,
                NotificationUserInvitationEmailSender>(
                client =>
                {
                    client.BaseAddress =
                        notificationBaseUri;
                });

        return services;
    }
}