using JobPlatform.Domain.Entities.Interviews;
using JobPlatform.Infrastructure.Persistence;

namespace JobPlatform.Infrastructure.ReminderJobs;

using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public sealed class InterviewReminderJob : IInterviewReminderJob
{
    private readonly AppDbContext _db;
    private readonly INotificationPublisher _noti;

    public InterviewReminderJob(AppDbContext db, INotificationPublisher noti)
    {
        _db = db;
        _noti = noti;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var due = await _db.Interviews
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(i => i.JobApplication).ThenInclude(a => a.CandidateProfile)
            .Where(i =>
                i.Status != InterviewStatus.Cancelled &&
                !i.ReminderSent &&
                i.ReminderAt != null &&
                i.ReminderAt <= now &&
                i.StartAt > now) 
            .OrderBy(i => i.ReminderAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var i in due)
        {
            // notify candidate
            await _noti.NotifyAsync(
                userId: i.JobApplication.CandidateProfile.UserId,
                type: "InterviewReminder",
                title: $"Interview reminder: {i.JobApplication.JobPost.Title}",
                body: $"Your interview starts at {i.StartAt:O}. Location: {i.Location ?? "N/A"}",
                data: new { interviewId = i.Id },
                targetUrl: $"/candidates/me/interviews?focus={i.Id}",
                sendEmail: true,
                ct: ct);

            // notify employer 
            await _noti.NotifyAsync(
                userId: i.JobApplication.JobPost.EmployerProfile.UserId,
                type: "InterviewReminder",
                title: $"Interview reminder: {i.JobApplication.JobPost.Title}",
                body: $"Interview starts at {i.StartAt:O}. Candidate: {i.JobApplication.CandidateProfile.FullName}",
                data: new { interviewId = i.Id },
                targetUrl: $"/employer/interviews/{i.Id}",
                sendEmail: false,
                ct: ct);

            i.ReminderSent = true;
            i.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);
    }
}
