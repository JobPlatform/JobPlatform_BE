namespace JobPlatform.Application.Skills.DTOs;

public sealed record SkillCategoryDto(
    Guid Id,
    Guid DomainId,
    string DomainCode,
    string Code,
    string Name
);