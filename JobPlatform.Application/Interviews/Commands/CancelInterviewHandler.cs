using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities.Interviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Commands;

public sealed class CancelInterviewHandler : IRequestHandler<CancelInterviewCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _noti;

    public CancelInterviewHandler(IAppDbContext db, ICurrentUserService currentUser, INotificationPublisher noti)
    {
        _db = db; _currentUser = currentUser; _noti = noti;
    }

    public async Task<Unit> Handle(CancelInterviewCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var interview = await _db.Interviews
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(i => i.JobApplication).ThenInclude(a => a.CandidateProfile)
            .FirstOrDefaultAsync(i => i.Id == request.InterviewId, ct);

        if (interview is null) throw new NotFoundException("Interview not found");
        if (interview.JobApplication.JobPost.EmployerProfile.UserId != userId) throw new ForbiddenException();

        if (interview.Status == InterviewStatus.Cancelled) return Unit.Value;

        interview.Status = InterviewStatus.Cancelled;
        interview.Note = string.IsNullOrWhiteSpace(request.Reason) ? interview.Note : request.Reason.Trim();
        interview.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _noti.NotifyAsync(
            userId: interview.JobApplication.CandidateProfile.UserId,
            type: "InterviewCancelled",
            title: $"Interview cancelled: {interview.JobApplication.JobPost.Title}",
            body: $"Reason: {request.Reason ?? "N/A"}",
            data: new { interviewId = interview.Id },
            targetUrl: $"/candidates/me/interviews",
            sendEmail: true,
            ct: ct);
        return Unit.Value;
    }
}
