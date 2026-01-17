using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Interviews.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Interviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Interviews.Queries;

public sealed class GetEmployerInterviewsHandler
    : IRequestHandler<GetEmployerInterviewsQuery, PagedResult<EmployerInterviewListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetEmployerInterviewsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<EmployerInterviewListItemDto>> Handle(GetEmployerInterviewsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        if (request.To < request.From) throw new BadRequestException("'to' must be >= 'from'");
        if ((request.To - request.From).TotalDays > 366) throw new BadRequestException("Range too large (max 366 days)");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var q = _db.Interviews
            .AsNoTracking()
            .Include(i => i.JobApplication).ThenInclude(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(i => i.JobApplication).ThenInclude(a => a.CandidateProfile)
            .Where(i => i.JobApplication.JobPost.EmployerProfile.UserId == userId)
            .Where(i => i.StartAt >= request.From && i.StartAt <= request.To);

        if (!request.IncludeCancelled)
            q = q.Where(i => i.Status != InterviewStatus.Cancelled);

        q = q.OrderBy(i => i.StartAt);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new EmployerInterviewListItemDto(
                i.Id,
                i.JobApplicationId,
                i.JobApplication.JobPostId,
                i.JobApplication.JobPost.Title,
                i.JobApplication.CandidateProfileId,
                i.JobApplication.CandidateProfile.FullName,
                i.StartAt,
                i.DurationMinutes,
                i.Location,
                i.MeetingUrl,
                i.Status
            ))
            .ToListAsync(ct);

        return new PagedResult<EmployerInterviewListItemDto>(items, page, pageSize, total);
    }
}
