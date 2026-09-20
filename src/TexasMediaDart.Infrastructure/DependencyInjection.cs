using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Infrastructure.Http;
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

        return services;
    }
}