namespace JobPlatform.Application.Jobs.DTOs;

public sealed record JobRequirementDto(
    Guid SkillId,
    string SkillName,
    string CategoryName,
    int RequiredLevel,
    bool IsMustHave,
    int Weight,
    string? RequirementDescription
);