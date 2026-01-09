using JobPlatform.Domain.Entities.Applications;

namespace JobPlatform.Domain.Entities.Interviews;


public enum InterviewStatus { Scheduled = 0, Confirmed = 1, Rescheduled = 2, Cancelled = 3, Completed = 4 }

public class Interview : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = default!;

    public DateTimeOffset ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    // Call (MVP): room id/link (mock hoặc provider)
    public string? MeetingRoomId { get; set; }
    public string? MeetingLink { get; set; }

    public ICollection<InterviewParticipant> Participants { get; set; } = new List<InterviewParticipant>();
}

public enum InterviewParticipantRole { Candidate = 0, Recruiter = 1, Interviewer = 2 }

public class InterviewParticipant
{
    public Guid InterviewId { get; set; }
    public Interview Interview { get; set; } = default!;

    public Guid UserId { get; set; }
    public InterviewParticipantRole Role { get; set; }
}