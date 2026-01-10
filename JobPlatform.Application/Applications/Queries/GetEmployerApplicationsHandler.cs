using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Queries;

public sealed class GetEmployerApplicationsHandler
    : IRequestHandler<GetEmployerApplicationsQuery, PagedResult<EmployerApplicationKanbanItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetEmployerApplicationsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<EmployerApplicationKanbanItemDto>> Handle(GetEmployerApplicationsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        // All applications where job belongs to employer
        var q = _db.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(a => a.CandidateProfile)
            .Where(a => a.JobPost.EmployerProfile.UserId == userId)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            if (!Enum.IsDefined(typeof(ApplicationStatus), request.Status.Value))
                throw new BadRequestException("Invalid status");

            var st = (ApplicationStatus)request.Status.Value;
            q = q.Where(a => a.Status == st);
        }

        // Sorting
        q = request.Sort?.ToLowerInvariant() switch
        {
            "applied_asc" => q.OrderBy(a => a.AppliedAt),
            "status_changed_desc" => q.OrderByDescending(a => a.StatusChangedAt ?? a.AppliedAt),
            _ => q.OrderByDescending(a => a.AppliedAt) // applied_desc default
        };

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new EmployerApplicationKanbanItemDto(
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

        return new PagedResult<EmployerApplicationKanbanItemDto>(items, page, pageSize, total);
    }
}
