using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Queries;

public sealed class GetApplicationsByJobHandler : IRequestHandler<GetApplicationsByJobQuery, PagedResult<ApplicationListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetApplicationsByJobHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ApplicationListItemDto>> Handle(GetApplicationsByJobQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        // ensure job belongs to employer
        var job = await _db.JobPosts
            .Include(j => j.EmployerProfile)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, ct);

        if (job is null) throw new NotFoundException("Job not found");
        if (job.EmployerProfile.UserId != userId) throw new ForbiddenException();

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.JobApplications
            .AsNoTracking()
            .Where(a => a.JobPostId == request.JobId)
            .Include(a => a.JobPost)
            .Include(a => a.CandidateProfile)
            .OrderByDescending(a => a.AppliedAt)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            if (!Enum.IsDefined(typeof(ApplicationStatus), request.Status.Value))
                throw new BadRequestException("Invalid status");

            var st = (ApplicationStatus)request.Status.Value;
            q = q.Where(a => a.Status == st);
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApplicationListItemDto(
                a.Id,
                a.JobPostId,
                a.JobPost.Title,
                a.CandidateProfileId,
                a.CandidateProfile.FullName,
                a.CandidateProfile.Headline,
                a.Status,
                a.AppliedAt,
                a.StatusChangedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<ApplicationListItemDto>(items, page, pageSize, total);
    }
}
