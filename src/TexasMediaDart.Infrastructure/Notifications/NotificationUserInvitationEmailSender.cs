using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TexasMediaDart.Application.UserInvitations.Abstractions;

namespace TexasMediaDart.Infrastructure.Notifications;

public sealed class NotificationUserInvitationEmailSender
    : IUserInvitationEmailSender
{
    private const string UserInvitationEndpoint =
        "internal/notifications/user-invitation";

    private readonly HttpClient _httpClient;
    private readonly NotificationServiceOptions _options;
    private readonly ILogger<NotificationUserInvitationEmailSender>
        _logger;
    private readonly string _frontendBaseUrl;

    public NotificationUserInvitationEmailSender(
        HttpClient httpClient,
        IOptions<NotificationServiceOptions> options,
        IConfiguration configuration,
        ILogger<NotificationUserInvitationEmailSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _frontendBaseUrl =
            configuration["Frontend:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException(
                "Frontend:BaseUrl is not configured.");

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "NotificationService:ApiKey is not configured.");
        }
    }

    public async Task SendAsync(
        string email,
        string invitationToken,
        DateTime expiresUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            invitationToken);

        var encodedToken =
            Uri.EscapeDataString(invitationToken);

        var invitationUrl =
            $"{_frontendBaseUrl}/#/accept-invitation" +
            $"?token={encodedToken}";

        var request =
            new UserInvitationNotificationRequest(
                email,
                invitationUrl,
                expiresUtc);

        using var httpRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                UserInvitationEndpoint)
            {
                Content =
                    JsonContent.Create(request)
            };

        httpRequest.Headers.Add(
            "X-API-Key",
            _options.ApiKey);

        _logger.LogInformation(
            "Submitting user invitation notification for {Email}.",
            email);

        using var response =
            await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "User invitation notification accepted for {Email}. " +
                "Status code: {StatusCode}.",
                email,
                (int)response.StatusCode);

            return;
        }

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        _logger.LogError(
            "Notification service rejected user invitation for {Email}. " +
            "Status code: {StatusCode}. Response: {ResponseBody}",
            email,
            (int)response.StatusCode,
            responseBody);

        throw new HttpRequestException(
            $"Notification service returned HTTP " +
            $"{(int)response.StatusCode} " +
            $"({response.StatusCode}).");
    }

    private sealed record UserInvitationNotificationRequest(
        string RecipientEmail,
        string InvitationUrl,
        DateTime ExpiresUtc);
}