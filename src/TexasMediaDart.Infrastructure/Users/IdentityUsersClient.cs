using System.Net.Http.Json;
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
}