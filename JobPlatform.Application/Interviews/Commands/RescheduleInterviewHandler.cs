using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities.Interviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Commands;

public sealed class RescheduleInterviewHandler : IRequestHandler<RescheduleInterviewCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _noti;

    public RescheduleInterviewHandler(IAppDbContext db, ICurrentUserService currentUser, INotificationPublisher noti)
    {
        _db = db; _currentUser = currentUser; _noti = noti;
    }

    public async Task<Unit> Handle(RescheduleInterviewCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var interview = await _db.Interviews
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(i => i.JobApplication).ThenInclude(a => a.CandidateProfile)
            .FirstOrDefaultAsync(i => i.Id == request.InterviewId, ct);

        if (interview is null) throw new NotFoundException("Interview not found");
        if (interview.JobApplication.JobPost.EmployerProfile.UserId != userId) throw new ForbiddenException();

        if (request.Dto.NewStartAt <= DateTimeOffset.UtcNow.AddMinutes(5))
            throw new BadRequestException("NewStartAt must be in the future");

        var duration = request.Dto.DurationMinutes ?? interview.DurationMinutes;
        if (duration is < 15 or > 240)
            throw new BadRequestException("DurationMinutes must be between 15 and 240");

        var reminderMinutes = request.Dto.ReminderMinutesBefore ?? (interview.ReminderAt.HasValue
            ? (int)Math.Max(5, (interview.StartAt - interview.ReminderAt.Value).TotalMinutes)
            : 60);

        if (reminderMinutes is < 5 or > 10080)
            reminderMinutes = 60;

        interview.StartAt = request.Dto.NewStartAt;
        interview.DurationMinutes = duration;
        interview.Location = string.IsNullOrWhiteSpace(request.Dto.Location) ? interview.Location : request.Dto.Location.Trim();
        interview.MeetingUrl = string.IsNullOrWhiteSpace(request.Dto.MeetingUrl) ? interview.MeetingUrl : request.Dto.MeetingUrl.Trim();
        interview.Note = string.IsNullOrWhiteSpace(request.Dto.Note) ? interview.Note : request.Dto.Note.Trim();

        interview.Status = InterviewStatus.Rescheduled;
        interview.ReminderAt = interview.StartAt.AddMinutes(-reminderMinutes);
        interview.ReminderSent = false;
        interview.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _noti.NotifyAsync(
            userId: interview.JobApplication.CandidateProfile.UserId,
            type: "InterviewRescheduled",
            title: $"Interview rescheduled: {interview.JobApplication.JobPost.Title}",
            body: $"New time: {interview.StartAt:O} ({interview.DurationMinutes} mins).",
            data: new { interviewId = interview.Id },
            targetUrl: $"/candidates/me/interviews?focus={interview.Id}",
            sendEmail: true,
            ct: ct);
        return Unit.Value;
    }
}
