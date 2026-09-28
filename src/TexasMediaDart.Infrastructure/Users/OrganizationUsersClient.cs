using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
                await ReadErrorMessageAsync(
                    response,
                    "Unable to create the organization user.",
                    cancellationToken);

            throw new BadRequestException(errorMessage);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var errorMessage =
                await ReadErrorMessageAsync(
                    response,
                    "The identity user already belongs to an organization.",
                    cancellationToken);

            throw new ConflictException(errorMessage);
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

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        var content =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return fallbackMessage;
        }

        try
        {
            using var document = JsonDocument.Parse(content);

            if (document.RootElement.ValueKind ==
                JsonValueKind.Object)
            {
                if (document.RootElement.TryGetProperty(
                        "detail",
                        out var detail) &&
                    detail.ValueKind == JsonValueKind.String)
                {
                    var message = detail.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }
                }

                if (document.RootElement.TryGetProperty(
                        "message",
                        out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    var message = messageElement.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }
                }

                if (document.RootElement.TryGetProperty(
                        "title",
                        out var title) &&
                    title.ValueKind == JsonValueKind.String)
                {
                    var message = title.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }
                }
            }

            if (document.RootElement.ValueKind ==
                JsonValueKind.String)
            {
                var message = document.RootElement.GetString();

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
        }
        catch (JsonException)
        {
            // The downstream response was not JSON.
            // Return the response body as the error message.
        }

        return content;
    }
}