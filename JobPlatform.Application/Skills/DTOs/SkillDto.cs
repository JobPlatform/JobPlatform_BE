namespace JobPlatform.Application.Skills.DTOs;

public sealed record SkillDto(
    Guid Id,
    Guid CategoryId,
    string DomainCode,
    string CategoryCode,
    string Code,
    string Name,
    bool IsActive
);