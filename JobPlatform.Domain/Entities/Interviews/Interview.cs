using JobPlatform.Domain.Entities.Applications;

namespace JobPlatform.Domain.Entities.Interviews;


public enum InterviewStatus
{
    Scheduled = 1,
    Confirmed = 2,
    RescheduleRequested = 3,
    Rescheduled = 4,
    Cancelled = 5
}

public class Interview : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = default!;

    public DateTimeOffset StartAt { get; set; }
    public int DurationMinutes { get; set; } = 45;

    public string? Location { get; set; }        // "Google Meet" 
    
    public string? MeetingUrl { get; set; }      

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public string? Note { get; set; }            
    public DateTimeOffset? ConfirmedAt { get; set; }

    // Reminder
    public DateTimeOffset? ReminderAt { get; set; }  
    public bool ReminderSent { get; set; }
}
