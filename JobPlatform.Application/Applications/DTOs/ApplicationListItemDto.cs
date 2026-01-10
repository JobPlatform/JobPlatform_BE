namespace JobPlatform.Application.Applications.DTOs;

public sealed record ApplicationListItemDto(
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