using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities.Interviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Commands;

public sealed class RequestRescheduleHandler : IRequestHandler<RequestRescheduleCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _noti;

    public RequestRescheduleHandler(IAppDbContext db, ICurrentUserService currentUser, INotificationPublisher noti)
    {
        _db = db; _currentUser = currentUser; _noti = noti;
    }

    public async Task<Unit> Handle(RequestRescheduleCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var interview = await _db.Interviews
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(i => i.JobApplication).ThenInclude(a => a.CandidateProfile)
            .FirstOrDefaultAsync(i => i.Id == request.InterviewId, ct);

        if (interview is null) throw new NotFoundException("Interview not found");
        if (interview.JobApplication.CandidateProfile.UserId != userId) throw new ForbiddenException();

        if (interview.Status is InterviewStatus.Cancelled)
            throw new BadRequestException("Interview is cancelled");

        interview.Status = InterviewStatus.RescheduleRequested;
        interview.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _noti.NotifyAsync(
            userId: interview.JobApplication.JobPost.EmployerProfile.UserId,
            type: "InterviewRescheduleRequested",
            title: $"Reschedule requested: {interview.JobApplication.JobPost.Title}",
            body: $"Candidate requested reschedule. Suggested: {request.Dto.SuggestedStartAt?.ToString("O") ?? "N/A"}. Reason: {request.Dto.Reason ?? "N/A"}",
            data: new { interviewId = interview.Id, suggestedStartAt = request.Dto.SuggestedStartAt, reason = request.Dto.Reason },
            targetUrl: $"/employer/interviews/{interview.Id}",
            sendEmail: true,
            ct: ct);
        return Unit.Value;
    }
}
