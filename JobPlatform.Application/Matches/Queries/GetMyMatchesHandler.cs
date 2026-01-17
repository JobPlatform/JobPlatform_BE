using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Matches.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Matches.Queries;

public sealed class GetMyMatchesHandler : IRequestHandler<GetMyMatchesQuery, PagedResult<CandidateJobMatchDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyMatchesHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<CandidateJobMatchDto>> Handle(GetMyMatchesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var candidate = await _db.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (candidate is null) throw new NotFoundException("Candidate profile not found");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.JobMatches
            .AsNoTracking()
            .Where(m => m.CandidateProfileId == candidate.Id)
            .Include(m => m.JobPost).ThenInclude(j => j.EmployerProfile)
            .AsQueryable();

        if (request.JobId.HasValue)
            q = q.Where(m => m.JobPostId == request.JobId.Value);

        q = q.OrderByDescending(m => m.Score).ThenByDescending(m => m.CreatedAt);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new CandidateJobMatchDto(
                m.Id,
                m.JobPostId,
                m.JobPost.Title,
                m.JobPost.EmployerProfile.CompanyName,
                m.Score,
                m.IsNotified,
                m.NotifiedAt,
                m.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<CandidateJobMatchDto>(items, page, pageSize, total);
    }
}
