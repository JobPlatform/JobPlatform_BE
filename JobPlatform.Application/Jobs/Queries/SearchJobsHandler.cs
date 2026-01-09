using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Jobs.Queries;


    public sealed class SearchJobsHandler : IRequestHandler<SearchJobsQuery, PagedResult<JobListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public SearchJobsHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<JobListItemDto>> Handle(SearchJobsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var query = _db.JobPosts
            .AsNoTracking()
            .Include(j => j.EmployerProfile)
            .Where(j => j.Status == JobStatus.Published);

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.Trim();
            query = query.Where(j =>
                EF.Functions.Like(j.Title, $"%{q}%") ||
                EF.Functions.Like(j.Description, $"%{q}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            var loc = request.Location.Trim();
            query = query.Where(j => j.Location != null && EF.Functions.Like(j.Location, $"%{loc}%"));
        }

        if (request.WorkMode.HasValue)
        {
            var wm = (WorkMode)request.WorkMode.Value;
            query = query.Where(j => j.WorkMode == wm);
        }

        if (request.SalaryMin.HasValue)
        {
            var min = request.SalaryMin.Value;
            query = query.Where(j =>
                (j.SalaryMax.HasValue && j.SalaryMax.Value >= min) ||
                (j.SalaryMax == null && j.SalaryMin.HasValue && j.SalaryMin.Value >= min));
        }

        if (request.SalaryMax.HasValue)
        {
            var max = request.SalaryMax.Value;
            query = query.Where(j =>
                (j.SalaryMin.HasValue && j.SalaryMin.Value <= max) ||
                (j.SalaryMin == null && j.SalaryMax.HasValue && j.SalaryMax.Value <= max));
        }

        if (request.Skills is { Count: > 0 })
        {
            var skills = request.Skills
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            query = query.Where(j =>
                j.SkillRequirements.Any(r => skills.Contains(r.Skill.Name.ToLower())));
        }

        // sort
        query = request.Sort?.ToLowerInvariant() switch
        {
            "salary_desc" => query.OrderByDescending(j => j.SalaryMax ?? j.SalaryMin ?? 0),
            "salary_asc" => query.OrderBy(j => j.SalaryMin ?? j.SalaryMax ?? 0),
            _ => query.OrderByDescending(j => j.PublishedAt ?? j.CreatedAt)
        };

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<JobListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<JobListItemDto>(items, page, pageSize, total);
    }
}