using JobPlatform.Domain.Entities.Profiles;

namespace JobPlatform.Domain.Entities.Jobs;

public enum JobStatus { Draft = 0, Published = 1, Closed = 2 }
public enum WorkMode { Onsite = 0, Hybrid = 1, Remote = 2 }

public class JobPost : BaseEntity
{
    public Guid EmployerProfileId { get; set; }
    public EmployerProfile EmployerProfile { get; set; } = default!;

    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }

    public ICollection<JobSkillRequirement> SkillRequirements { get; set; } = new List<JobSkillRequirement>();
}

public class JobSkillRequirement
{
    public Guid JobPostId { get; set; }
    public JobPost JobPost { get; set; } = default!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = default!;

    public int RequiredLevel { get; set; } // 1-5
    public bool IsMustHave { get; set; }
    public int Weight { get; set; } = 1;

    public string? RequirementDescription { get; set; } // mô tả thêm
}