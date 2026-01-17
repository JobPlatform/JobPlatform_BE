namespace JobPlatform.Application.Interviews.DTOs;

public sealed record CreateInterviewDto(
    DateTimeOffset StartAt,
    int DurationMinutes,
    string? Location,
    string? MeetingUrl,
    string? Note,
    int ReminderMinutesBefore 
);
