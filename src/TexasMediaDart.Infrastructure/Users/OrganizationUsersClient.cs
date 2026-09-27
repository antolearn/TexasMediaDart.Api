using System.Net;
using System.Net.Http.Json;
using TexasMediaDart.Application.Common.Exceptions;
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
        string sortBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var queryParameters =
            new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}",
                $"sortBy={Uri.EscapeDataString(sortBy)}",
                $"sortDirection={Uri.EscapeDataString(sortDirection)}"
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
                $"isActive={
                    isActive.Value.ToString().ToLowerInvariant()}");
        }

        if (isApproved.HasValue)
        {
            queryParameters.Add(
                $"isApproved={
                    isApproved.Value.ToString().ToLowerInvariant()}");
        }

        var requestUri =
            $"api/users?{string.Join("&", queryParameters)}";

        using var response =
            await _httpClient.GetAsync(
                requestUri,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Access to organization users is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new BadRequestException(errorMessage);
        }

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OrganizationUserSearchResultDto>(
                    cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "Organization API returned an empty users response.");
    }

    public async Task<IReadOnlyList<Guid>>
        GetCandidateIdentityUserIdsAsync(
            Guid? identityUserId,
            bool? isActive,
            bool? isApproved,
            CancellationToken cancellationToken = default)
    {
        var queryParameters = new List<string>();

        if (identityUserId.HasValue)
        {
            queryParameters.Add(
                $"identityUserId={Uri.EscapeDataString(
                    identityUserId.Value.ToString())}");
        }

        if (isActive.HasValue)
        {
            queryParameters.Add(
                $"isActive={
                    isActive.Value.ToString().ToLowerInvariant()}");
        }

        if (isApproved.HasValue)
        {
            queryParameters.Add(
                $"isApproved={
                    isApproved.Value.ToString().ToLowerInvariant()}");
        }

        var requestUri =
            "api/users/candidate-identity-ids";

        if (queryParameters.Count > 0)
        {
            requestUri +=
                $"?{string.Join("&", queryParameters)}";
        }

        using var response =
            await _httpClient.GetAsync(
                requestUri,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Access to organization users is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new BadRequestException(errorMessage);
        }

        response.EnsureSuccessStatusCode();

        var identityUserIds =
            await response.Content
                .ReadFromJsonAsync<List<Guid>>(
                    cancellationToken: cancellationToken);

        return identityUserIds is null
            ? Array.Empty<Guid>()
            : identityUserIds;
    }

    public async Task<OrganizationUserSearchResultDto>
        SearchByIdentityIdsAsync(
            OrganizationUserSearchByIdentityIdsRequest request,
            CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/users/search-by-identity-ids",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Access to organization users is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new BadRequestException(errorMessage);
        }

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OrganizationUserSearchResultDto>(
                    cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "Organization API returned an empty users response.");
    }

    public async Task<OrganizationUserDto> CreateAsync(
        Guid identityUserId,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            identityUserId
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/users",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Creating organization users is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new BadRequestException(errorMessage);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new ConflictException(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "The identity user already belongs to an organization."
                    : errorMessage);
        }

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OrganizationUserDto>(
                    cancellationToken: cancellationToken);

        return result
            ?? throw new InvalidOperationException(
                "Organization API returned an empty create user response.");
    }
}