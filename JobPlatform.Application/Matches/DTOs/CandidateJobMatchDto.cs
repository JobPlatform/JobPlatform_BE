namespace JobPlatform.Application.Matches.DTOs;

public sealed record CandidateJobMatchDto(
    Guid MatchId,
    Guid JobPostId,
    string JobTitle,
    string CompanyName,
    decimal Score,
    bool IsNotified,
    DateTimeOffset? NotifiedAt,
    DateTimeOffset CreatedAt
);