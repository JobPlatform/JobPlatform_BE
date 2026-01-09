namespace JobPlatform.Application.Candidates.DTOs;

public sealed record CandidateProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string? Headline,
    string? Location,
    decimal? ExpectedSalaryMin,
    decimal? ExpectedSalaryMax,
    bool PreferRemote,
    string? CvFileName,
    string? CvStoragePath,
    DateTimeOffset? CvUploadedAt,
    List<CandidateSkillDto> Skills
);