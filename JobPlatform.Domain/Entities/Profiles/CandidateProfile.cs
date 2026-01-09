namespace JobPlatform.Domain.Entities.Profiles;
public class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public required string FullName { get; set; }
    public string? Headline { get; set; }
    public string? Location { get; set; }

    public decimal? ExpectedSalaryMin { get; set; }
    public decimal? ExpectedSalaryMax { get; set; }
    public bool PreferRemote { get; set; }

    // CV upload metadata
    public string? CvFileName { get; set; }     // "LeTien_CV.pdf"
    public string? CvContentType { get; set; }  // "application/pdf"
    public long? CvFileSize { get; set; }
    public string? CvStoragePath { get; set; }  // "/uploads/cv/{userId}/{file}.pdf" hoặc URL
    public DateTimeOffset? CvUploadedAt { get; set; }

    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
}

public class CandidateSkill
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = default!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = default!;

    public int Level { get; set; } // 1-5
    public decimal Years { get; set; } // 0.5, 1.0...
}