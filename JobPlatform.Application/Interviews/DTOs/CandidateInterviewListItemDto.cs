using JobPlatform.Domain.Entities.Interviews;

namespace JobPlatform.Application.Interviews.DTOs;

public sealed record CandidateInterviewListItemDto(
    Guid Id,
    Guid JobApplicationId,
    Guid JobPostId,
    string JobTitle,
    string CompanyName,
    DateTimeOffset StartAt,
    int DurationMinutes,
    string? Location,
    string? MeetingUrl,
    InterviewStatus Status,
    DateTimeOffset? ReminderAt,
    bool ReminderSent
);