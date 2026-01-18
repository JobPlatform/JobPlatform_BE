using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Interviews.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Queries;

public sealed class GetCandidateInterviewDetailHandler
    : IRequestHandler<GetCandidateInterviewDetailQuery, InterviewDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetCandidateInterviewDetailHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<InterviewDetailDto> Handle(GetCandidateInterviewDetailQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var interview = await _db.Interviews
            .AsNoTracking()
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(i => i.JobApplication).ThenInclude(a => a.CandidateProfile)
            .FirstOrDefaultAsync(i => i.Id == request.InterviewId, ct);

        if (interview is null) throw new NotFoundException("Interview not found");

        var candidateUserId = interview.JobApplication.CandidateProfile.UserId;
        if (candidateUserId != userId) throw new ForbiddenException();

        return new InterviewDetailDto(
            interview.Id,
            interview.JobApplicationId,
            interview.JobApplication.JobPostId,
            interview.JobApplication.JobPost.Title,
            candidateUserId,
            interview.JobApplication.JobPost.EmployerProfile.UserId,
            interview.StartAt,
            interview.DurationMinutes,
            interview.Location,
            interview.MeetingUrl,
            interview.Status,
            interview.Note,
            interview.ConfirmedAt,
            interview.ReminderAt,
            interview.ReminderSent
        );
    }
}
