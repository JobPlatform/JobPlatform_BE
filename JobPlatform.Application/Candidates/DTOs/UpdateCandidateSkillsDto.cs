namespace JobPlatform.Application.Candidates.DTOs;

public sealed record UpdateCandidateSkillsDto(
    List<UpsertCandidateSkillItemDto> Skills
);