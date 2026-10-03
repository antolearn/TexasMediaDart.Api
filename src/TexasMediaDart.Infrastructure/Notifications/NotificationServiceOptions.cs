namespace TexasMediaDart.Infrastructure.Notifications;

public sealed class NotificationServiceOptions
{
    public const string SectionName = "NotificationService";

    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;
}