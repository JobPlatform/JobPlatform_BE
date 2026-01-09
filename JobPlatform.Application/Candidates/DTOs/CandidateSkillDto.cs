namespace JobPlatform.Application.Candidates.DTOs;

public sealed record CandidateSkillDto(
    Guid SkillId,
    string SkillCode,
    string SkillName,
    int Level,
    decimal Years
);