using JobPlatform.Domain.Entities.Jobs;
using JobPlatform.Domain.Entities.Profiles;

namespace JobPlatform.Domain.Entities.Applications;

public class JobApplication : BaseEntity
{
    public Guid JobPostId { get; set; }
    public JobPost JobPost { get; set; } = default!;

    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = default!;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public string? CvFilePath { get; set; } // upload sau
}

public class JobRecommendation
{
    public Guid JobPostId { get; set; }
    public JobPost JobPost { get; set; } = default!;

    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = default!;

    public double Score { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? NotifiedAt { get; set; }
}