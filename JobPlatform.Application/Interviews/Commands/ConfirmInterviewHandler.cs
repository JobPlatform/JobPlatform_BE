using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities.Interviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Commands;

public sealed class ConfirmInterviewHandler : IRequestHandler<ConfirmInterviewCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _noti;

    public ConfirmInterviewHandler(IAppDbContext db, ICurrentUserService currentUser, INotificationPublisher noti)
    {
        _db = db; _currentUser = currentUser; _noti = noti;
    }

    public async Task<Unit> Handle(ConfirmInterviewCommand request, CancellationToken ct)
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

        if (interview.StartAt <= DateTimeOffset.UtcNow)
            throw new BadRequestException("Cannot confirm past interview");

        interview.Status = InterviewStatus.Confirmed;
        interview.ConfirmedAt = DateTimeOffset.UtcNow;
        interview.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        // notify employer
        await _noti.NotifyAsync(
            userId: interview.JobApplication.JobPost.EmployerProfile.UserId,
            type: "InterviewConfirmed",
            title: $"Interview confirmed: {interview.JobApplication.JobPost.Title}",
            body: $"Candidate confirmed the interview at {interview.StartAt:O}.",
            data: new { interviewId = interview.Id, applicationId = interview.JobApplicationId },
            targetUrl: $"/employer/interviews/{interview.Id}",
            sendEmail: true,
            ct: ct);
        return Unit.Value;
    }
}
