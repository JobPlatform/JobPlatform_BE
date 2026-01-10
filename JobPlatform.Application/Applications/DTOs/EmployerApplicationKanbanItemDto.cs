using JobPlatform.Domain.Entities;

namespace JobPlatform.Application.Applications.DTOs;

public sealed record EmployerApplicationKanbanItemDto(
    Guid Id,
    Guid JobPostId,
    string JobTitle,
    Guid CandidateProfileId,
    string CandidateName,
    string? CandidateHeadline,
    ApplicationStatus Status,
    DateTimeOffset AppliedAt,
    DateTimeOffset? StatusChangedAt
);