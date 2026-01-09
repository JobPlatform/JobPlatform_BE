namespace JobPlatform.Application.Candidates.DTOs;

public sealed record UpdateCandidateProfileDto(
    string FullName,
    string? Headline,
    string? Location,
    decimal? ExpectedSalaryMin,
    decimal? ExpectedSalaryMax,
    bool PreferRemote
);