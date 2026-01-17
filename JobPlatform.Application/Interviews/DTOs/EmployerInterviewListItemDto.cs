using JobPlatform.Domain.Entities.Interviews;

namespace JobPlatform.Application.Interviews.DTOs;

public sealed record EmployerInterviewListItemDto(
    Guid Id,
    Guid JobApplicationId,
    Guid JobPostId,
    string JobTitle,
    Guid CandidateProfileId,
    string CandidateName,
    DateTimeOffset StartAt,
    int DurationMinutes,
    string? Location,
    string? MeetingUrl,
    InterviewStatus Status
);