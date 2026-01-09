namespace JobPlatform.Domain.Entities;
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class SkillDomain : BaseEntity
{
    public required string Code { get; set; }     // "IT"
    public required string Name { get; set; }     // "Information Technology"
}

public class SkillCategory : BaseEntity
{
    public Guid DomainId { get; set; }
    public SkillDomain Domain { get; set; } = default!;

    public required string Code { get; set; }     // "LANG", "DB", "FRAMEWORK"
    public required string Name { get; set; }     // "Programming Languages"
}

public class Skill : BaseEntity
{
    public Guid CategoryId { get; set; }
    public SkillCategory Category { get; set; } = default!;

    public required string Code { get; set; }     // "C_SHARP", "JAVA"
    public required string Name { get; set; }     // "C#"
    public bool IsActive { get; set; } = true;
}