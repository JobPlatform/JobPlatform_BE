namespace JobPlatform.Application.Chat.DTOs;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderUserId,
    string Content,
    DateTimeOffset SentAt
);

public sealed record ChatSummaryDto(
    Guid ConversationId,
    Guid JobApplicationId,
    Guid OtherUserId,
    string OtherDisplayName,
    int UnreadCount,
    bool IsActive,
    MessageDto? LastMessage
);

public sealed record CursorPagedResult<T>(
    IReadOnlyList<T> Items,
    DateTimeOffset? NextCursorSentAt,
    Guid? NextCursorId
);