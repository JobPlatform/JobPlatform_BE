using JobPlatform.Domain.Entities.Interviews;

namespace JobPlatform.Application.Interviews.Commands;

using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class CreateInterviewHandler : IRequestHandler<CreateInterviewCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _noti;

    public CreateInterviewHandler(IAppDbContext db, ICurrentUserService currentUser, INotificationPublisher noti)
    {
        _db = db;
        _currentUser = currentUser;
        _noti = noti;
    }

    public async Task<Guid> Handle(CreateInterviewCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        if (request.Dto.DurationMinutes is < 15 or > 240)
            throw new BadRequestException("DurationMinutes must be between 15 and 240");

        if (request.Dto.StartAt <= DateTimeOffset.UtcNow.AddMinutes(5))
            throw new BadRequestException("StartAt must be in the future");

        var app = await _db.JobApplications
            .Include(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct);

        if (app is null) throw new NotFoundException("Application not found");
        if (app.JobPost.EmployerProfile.UserId != userId) throw new ForbiddenException();

        
        if (app.Status is ApplicationStatus.Applied)
            throw new BadRequestException("Move application to Reviewed before scheduling interview");

        
        var hasActive = await _db.Interviews.AnyAsync(i =>
            i.JobApplicationId == app.Id &&
            i.Status != InterviewStatus.Cancelled, ct);

        if (hasActive)
            throw new BadRequestException("This application already has an interview");

        var reminderMinutes = request.Dto.ReminderMinutesBefore is < 5 or > 10080 ? 60 : request.Dto.ReminderMinutesBefore;
        var reminderAt = request.Dto.StartAt.AddMinutes(-reminderMinutes);

        var interview = new Interview
        {
            JobApplicationId = app.Id,
            StartAt = request.Dto.StartAt,
            DurationMinutes = request.Dto.DurationMinutes,
            Location = string.IsNullOrWhiteSpace(request.Dto.Location) ? null : request.Dto.Location.Trim(),
            MeetingUrl = string.IsNullOrWhiteSpace(request.Dto.MeetingUrl) ? null : request.Dto.MeetingUrl.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Dto.Note) ? null : request.Dto.Note.Trim(),
            Status = InterviewStatus.Scheduled,
            ReminderAt = reminderAt,
            ReminderSent = false
        };

        _db.Interviews.Add(interview);

        
        if (app.Status < ApplicationStatus.Interview)
        {
            app.Status = ApplicationStatus.Interview;
            app.StatusChangedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        
        await _noti.NotifyAsync(
            userId: app.CandidateProfile.UserId,
            type: "InterviewScheduled",
            title: $"Interview scheduled: {app.JobPost.Title}",
            body: $"Time: {interview.StartAt:O} ({interview.DurationMinutes} mins).",
            data: new { interviewId = interview.Id, applicationId = app.Id, jobId = app.JobPostId },
            targetUrl: $"/candidates/me/interviews?focus={interview.Id}",
            sendEmail: true,
            ct: ct);

        return interview.Id;
    }
}
