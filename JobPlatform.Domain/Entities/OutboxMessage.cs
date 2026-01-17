namespace JobPlatform.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public required string Type { get; set; }           // "JobPublished"
    public required string PayloadJson { get; set; }    // { jobId, employerUserId }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ProcessedAt { get; set; }
    public int Attempt { get; set; }
    public string? Error { get; set; }
}
