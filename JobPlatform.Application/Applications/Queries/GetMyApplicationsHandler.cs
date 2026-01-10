using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Queries;

public sealed class GetMyApplicationsHandler : IRequestHandler<GetMyApplicationsQuery, PagedResult<MyApplicationListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyApplicationsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<MyApplicationListItemDto>> Handle(GetMyApplicationsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (candidate is null) throw new NotFoundException("Candidate profile not found");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.JobApplications
            .AsNoTracking()
            .Where(a => a.CandidateProfileId == candidate.Id)
            .Include(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
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
            .Select(a => new MyApplicationListItemDto(
                a.Id,
                a.JobPostId,
                a.JobPost.Title,
                a.JobPost.EmployerProfile.CompanyName,
                a.Status,
                a.AppliedAt,
                a.StatusChangedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<MyApplicationListItemDto>(items, page, pageSize, total);
    }
}
