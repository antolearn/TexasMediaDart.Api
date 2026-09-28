using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TexasMediaDart.Application.UserInvitations.Abstractions;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Infrastructure.Http;
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

        var organizationApiBaseUrl =
            configuration["Services:OrganizationApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Organization API base URL is not configured.");

        var identityApiBaseUrl =
            configuration["Services:IdentityApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Identity API base URL is not configured.");

        // Normal Organization API calls execute in the context
        // of the currently authenticated user.
        services
            .AddHttpClient<
                IOrganizationUsersClient,
                OrganizationUsersClient>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(organizationApiBaseUrl);
                })
            .AddHttpMessageHandler<BearerTokenHandler>();

        // Normal Identity API calls execute in the context
        // of the currently authenticated user.
        services
            .AddHttpClient<
                IIdentityUsersClient,
                IdentityUsersClient>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(identityApiBaseUrl);
                })
            .AddHttpMessageHandler<BearerTokenHandler>();

        // Invitation acceptance is anonymous.
        // Finalization adds the Identity service API key
        // explicitly inside IdentityUserInvitationsClient.
        services
            .AddHttpClient<
                IIdentityUserInvitationsClient,
                IdentityUserInvitationsClient>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(identityApiBaseUrl);
                });

        // Organization invitation membership acceptance
        // adds the Organization service API key explicitly
        // inside OrganizationUserInvitationsClient.
        services
            .AddHttpClient<
                IOrganizationUserInvitationsClient,
                OrganizationUserInvitationsClient>(
                client =>
                {
                    client.BaseAddress =
                        new Uri(organizationApiBaseUrl);
                });

        return services;
    }
}