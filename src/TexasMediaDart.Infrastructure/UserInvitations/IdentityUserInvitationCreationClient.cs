using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.UserInvitations.Abstractions;
using TexasMediaDart.Application.UserInvitations.Models;

namespace TexasMediaDart.Infrastructure.UserInvitations;

public sealed class IdentityUserInvitationCreationClient
    : IIdentityUserInvitationCreationClient
{
    private readonly HttpClient _httpClient;

    public IdentityUserInvitationCreationClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateUserInvitationDto> CreateAsync(
        string email,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            email,
            organizationId
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/user-invitations",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Unable to create the user invitation.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Identity API rejected the authenticated user.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Creating user invitations is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(
                await ReadErrorMessageAsync(
                    response,
                    "A pending invitation already exists.",
                    cancellationToken));
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<CreateUserInvitationDto>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Identity API returned an empty invitation creation response.");
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