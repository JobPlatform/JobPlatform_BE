namespace JobPlatform.Application.Jobs.DTOs;

public sealed record JobRequirementDto(
    Guid SkillId,
    int RequiredLevel,
    bool IsMustHave,
    int Weight,
    string? RequirementDescription
);