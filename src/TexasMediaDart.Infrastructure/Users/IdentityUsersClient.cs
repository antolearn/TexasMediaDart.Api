using System.Net;
using System.Net.Http.Json;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Users.Abstractions;
using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Infrastructure.Users;

public sealed class IdentityUsersClient
    : IIdentityUsersClient
{
    private readonly HttpClient _httpClient;

    public IdentityUsersClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<IdentityUserDto>> LookupAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return Array.Empty<IdentityUserDto>();
        }

        var request = new
        {
            userIds
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/users/lookup",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var users =
            await response.Content
                .ReadFromJsonAsync<List<IdentityUserDto>>(
                    cancellationToken: cancellationToken);

        return users is null
            ? Array.Empty<IdentityUserDto>()
            : users;
    }

    public async Task<IReadOnlyList<IdentityUserDto>>
        SearchByEmailAndIdsAsync(
            IReadOnlyCollection<Guid> userIds,
            string email,
            CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return Array.Empty<IdentityUserDto>();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Array.Empty<IdentityUserDto>();
        }

        var request = new
        {
            userIds,
            email = email.Trim()
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/users/search-by-email",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var users =
            await response.Content
                .ReadFromJsonAsync<List<IdentityUserDto>>(
                    cancellationToken: cancellationToken);

        return users is null
            ? Array.Empty<IdentityUserDto>()
            : users;
    }

    public async Task<IdentityUserDto?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var requestUri =
            $"api/users/by-email?email={
                Uri.EscapeDataString(email.Trim())}";

        using var response =
            await _httpClient.GetAsync(
                requestUri,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new BadRequestException(errorMessage);
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<IdentityUserDto>(
                cancellationToken: cancellationToken);
    }
}