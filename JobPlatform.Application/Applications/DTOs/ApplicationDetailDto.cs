namespace JobPlatform.Application.Applications.DTOs;

public sealed record ApplicationDetailDto(
    Guid Id,
    Guid JobPostId,
    string JobTitle,
    Guid CandidateProfileId,
    string CandidateName,
    string? CandidateHeadline,
    string? CandidateLocation,
    string? CandidateCvPath,
    ApplicationStatus Status,
    string? CoverLetter,
    string? StatusNote,
    DateTimeOffset AppliedAt,
    DateTimeOffset? StatusChangedAt
);