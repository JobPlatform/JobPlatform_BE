namespace JobPlatform.Domain.Entities.Notifications;
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public required string Type { get; set; } // "JobMatched", "NewMessage", ...
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string? TargetUrl { get; set; }
    public string? DataJson { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? EmailSentAt { get; set; }
}