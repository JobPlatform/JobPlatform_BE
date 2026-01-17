using JobPlatform.Domain.Entities.Interviews;

namespace JobPlatform.Application.Interviews.DTOs;

public sealed record InterviewDetailDto(
    Guid Id,
    Guid JobApplicationId,
    Guid JobPostId,
    string JobTitle,
    Guid CandidateUserId,
    Guid EmployerUserId,
    DateTimeOffset StartAt,
    int DurationMinutes,
    string? Location,
    string? MeetingUrl,
    InterviewStatus Status,
    string? Note,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? ReminderAt,
    bool ReminderSent
);