using System.Net.Http.Json;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Infrastructure.Users;

public sealed class OrganizationUsersClient
    : IOrganizationUsersClient
{
    private readonly HttpClient _httpClient;

    public OrganizationUsersClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrganizationUserSearchResultDto> SearchAsync(
        Guid? identityUserId,
        bool? isActive,
        bool? isApproved,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var queryParameters =
            new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

        if (identityUserId.HasValue)
        {
            queryParameters.Add(
                $"identityUserId={Uri.EscapeDataString(
                    identityUserId.Value.ToString())}");
        }

        if (isActive.HasValue)
        {
            queryParameters.Add(
                $"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        }

        if (isApproved.HasValue)
        {
            queryParameters.Add(
                $"isApproved={isApproved.Value.ToString().ToLowerInvariant()}");
        }

        var requestUri =
            $"api/users?{string.Join("&", queryParameters)}";

        using var response =
            await _httpClient.GetAsync(
                requestUri,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OrganizationUserSearchResultDto>(
                    cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "Organization API returned an empty users response.");
    }
}