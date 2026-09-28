using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Organizations.Abstractions;
using TexasMediaDart.Application.Organizations.Models;

namespace TexasMediaDart.Infrastructure.Organizations;

public sealed class OrganizationsClient : IOrganizationsClient
{
    private readonly HttpClient _httpClient;

    public OrganizationsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrentOrganizationDto> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        using var response =
            await _httpClient.GetAsync(
                "api/organizations/current",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Unable to determine the current organization.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Organization API rejected the authenticated user.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Access to the current organization is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(
                await ReadErrorMessageAsync(
                    response,
                    "No organization is associated with the authenticated user.",
                    cancellationToken));
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<CurrentOrganizationDto>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Organization API returned an empty current organization response.");
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