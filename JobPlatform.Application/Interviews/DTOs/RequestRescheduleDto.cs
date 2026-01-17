namespace JobPlatform.Application.Interviews.DTOs;

public sealed record RequestRescheduleDto(
    DateTimeOffset? SuggestedStartAt,
    string? Reason
);