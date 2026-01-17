using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Interviews.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Interviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Queries;

public sealed class GetMyInterviewsHandler
    : IRequestHandler<GetMyInterviewsQuery, PagedResult<CandidateInterviewListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyInterviewsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CandidateInterviewListItemDto>> Handle(GetMyInterviewsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (candidate is null) throw new NotFoundException("Candidate profile not found");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        // default range
        var now = DateTimeOffset.UtcNow;
        var from = request.From ?? (request.UpcomingOnly ? now.AddDays(-1) : now.AddDays(-30));
        var to = request.To ?? (request.UpcomingOnly ? now.AddDays(30) : now.AddDays(365));

        if (to < from) throw new BadRequestException("'to' must be >= 'from'");
        if ((to - from).TotalDays > 366) throw new BadRequestException("Range too large (max 366 days)");

        var q = _db.Interviews
            .AsNoTracking()
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Where(i => i.JobApplication.CandidateProfileId == candidate.Id)
            .Where(i => i.StartAt >= from && i.StartAt <= to);

        if (request.UpcomingOnly)
            q = q.Where(i => i.Status != InterviewStatus.Cancelled && i.StartAt >= now.AddMinutes(-30));

        q = q.OrderBy(i => i.StartAt);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new CandidateInterviewListItemDto(
                i.Id,
                i.JobApplicationId,
                i.JobApplication.JobPostId,
                i.JobApplication.JobPost.Title,
                i.JobApplication.JobPost.EmployerProfile.CompanyName,
                i.StartAt,
                i.DurationMinutes,
                i.Location,
                i.MeetingUrl,
                i.Status,
                i.ReminderAt,
                i.ReminderSent
            ))
            .ToListAsync(ct);

        return new PagedResult<CandidateInterviewListItemDto>(items, page, pageSize, total);
    }
}
