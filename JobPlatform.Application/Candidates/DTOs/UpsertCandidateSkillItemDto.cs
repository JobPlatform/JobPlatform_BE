namespace JobPlatform.Application.Candidates.DTOs;

public sealed record UpsertCandidateSkillItemDto(
    Guid SkillId,
    int Level,
    decimal Years
);