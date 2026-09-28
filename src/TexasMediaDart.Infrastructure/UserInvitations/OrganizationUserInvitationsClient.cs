using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.UserInvitations.Abstractions;
using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Infrastructure.UserInvitations;

public sealed class OrganizationUserInvitationsClient
    : IOrganizationUserInvitationsClient
{
    private const string ServiceApiKeyHeader =
        "X-TexasDart-Service-Key";

    private readonly HttpClient _httpClient;
    private readonly string _serviceApiKey;

    public OrganizationUserInvitationsClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _serviceApiKey =
            configuration[
                "ServiceAuthentication:OrganizationApiKey"]
            ?? throw new InvalidOperationException(
                "Organization service API key is not configured.");
    }

    public async Task<OrganizationUserDto> AcceptAsync(
        Guid organizationId,
        Guid identityUserId,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            organizationId,
            identityUserId
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/internal/user-invitations/accept")
            {
                Content = JsonContent.Create(body)
            };

        request.Headers.Add(
            ServiceApiKeyHeader,
            _serviceApiKey);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Organization invitation acceptance failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Organization API rejected the service credential.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Organization invitation acceptance is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(
                await ReadErrorMessageAsync(
                    response,
                    "The identity user belongs to a different organization.",
                    cancellationToken));
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<OrganizationUserDto>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Organization API returned an empty invitation acceptance response.");
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
            using var document =
                JsonDocument.Parse(content);

            if (document.RootElement.ValueKind ==
                JsonValueKind.Object)
            {
                foreach (var propertyName in
                         new[] { "detail", "message", "title" })
                {
                    if (document.RootElement.TryGetProperty(
                            propertyName,
                            out var property) &&
                        property.ValueKind ==
                            JsonValueKind.String)
                    {
                        var message = property.GetString();

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            return message;
                        }
                    }
                }
            }

            if (document.RootElement.ValueKind ==
                JsonValueKind.String)
            {
                return document.RootElement.GetString()
                    ?? fallbackMessage;
            }
        }
        catch (JsonException)
        {
            // Downstream response was not JSON.
        }

        return content;
    }
}