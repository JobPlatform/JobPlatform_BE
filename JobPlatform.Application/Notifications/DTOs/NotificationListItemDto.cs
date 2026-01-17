namespace JobPlatform.Application.Notifications.DTOs;

public sealed record NotificationListItemDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? DataJson,
    string? targetUrl,
    bool IsRead,
    DateTimeOffset CreatedAt
);