namespace JobPlatform.Application.Interviews.DTOs;

public sealed record RescheduleInterviewDto(
    DateTimeOffset NewStartAt,
    int? DurationMinutes,
    string? Location,
    string? MeetingUrl,
    string? Note,
    int? ReminderMinutesBefore
);
