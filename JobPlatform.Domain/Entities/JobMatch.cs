using JobPlatform.Domain.Entities.Jobs;

namespace JobPlatform.Domain.Entities;

public class JobMatch : BaseEntity
{
    public Guid JobPostId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public decimal Score { get; set; }

    public bool IsNotified { get; set; }
    public JobPost JobPost { get; set; }
    public DateTimeOffset? NotifiedAt { get; set; }
}
