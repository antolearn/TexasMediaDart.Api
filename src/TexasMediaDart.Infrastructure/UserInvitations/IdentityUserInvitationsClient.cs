using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.UserInvitations.Abstractions;
using TexasMediaDart.Application.UserInvitations.Models;

namespace TexasMediaDart.Infrastructure.UserInvitations;

public sealed class IdentityUserInvitationsClient
    : IIdentityUserInvitationsClient
{
    private const string ServiceApiKeyHeader =
        "X-TexasDart-Service-Key";

    private readonly HttpClient _httpClient;
    private readonly string _serviceApiKey;

    public IdentityUserInvitationsClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _serviceApiKey =
            configuration[
                "ServiceAuthentication:IdentityApiKey"]
            ?? throw new InvalidOperationException(
                "Identity service API key is not configured.");
    }

    public async Task<AcceptInvitationIdentityDto> AcceptAsync(
        string token,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            token,
            password,
            confirmPassword
        };

        using var response =
            await _httpClient.PostAsJsonAsync(
                "api/user-invitations/accept",
                request,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Invitation acceptance failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(
                await ReadErrorMessageAsync(
                    response,
                    "Invitation acceptance failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Gone)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "The invitation is no longer valid.",
                    cancellationToken));
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<AcceptInvitationIdentityDto>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Identity API returned an empty invitation acceptance response.");
    }

    public async Task<FinalizeInvitationDto> FinalizeAsync(
        Guid invitationId,
        Guid identityUserId,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            identityUserId
        };

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"api/user-invitations/{invitationId}/finalize")
            {
                Content = JsonContent.Create(request)
            };

        httpRequest.Headers.Add(
            ServiceApiKeyHeader,
            _serviceApiKey);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Invitation finalization failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Identity API rejected the service credential.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Invitation finalization is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(
                await ReadErrorMessageAsync(
                    response,
                    "The invitation was not found.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(
                await ReadErrorMessageAsync(
                    response,
                    "Invitation finalization failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Gone)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "The invitation is no longer valid.",
                    cancellationToken));
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<FinalizeInvitationDto>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Identity API returned an empty invitation finalization response.");
    }

    public async Task<IReadOnlyList<PendingInvitationDto>> GetPendingAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var requestUri =
            "api/user-invitations/pending" +
            $"?organizationId={organizationId}";

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Get,
                requestUri);

        httpRequest.Headers.Add(
            ServiceApiKeyHeader,
            _serviceApiKey);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Pending invitations query failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Identity API rejected the service credential.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Pending invitations query is forbidden.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PendingInvitationDto>>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Identity API returned an empty pending invitations response.");
    }
    public async Task<ResendInvitationDto> ResendAsync(
        Guid invitationId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var requestUri =
            $"api/user-invitations/{invitationId}/resend" +
            $"?organizationId={organizationId}";

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                requestUri);

        httpRequest.Headers.Add(
            ServiceApiKeyHeader,
            _serviceApiKey);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "Invitation resend failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Identity API rejected the service credential.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException(
                "Invitation resend is forbidden.");
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(
                await ReadErrorMessageAsync(
                    response,
                    "The invitation was not found.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(
                await ReadErrorMessageAsync(
                    response,
                    "Invitation resend failed.",
                    cancellationToken));
        }

        if (response.StatusCode == HttpStatusCode.Gone)
        {
            throw new BadRequestException(
                await ReadErrorMessageAsync(
                    response,
                    "The invitation is no longer valid.",
                    cancellationToken));
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ResendInvitationDto>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "Identity API returned an empty invitation resend response.");
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