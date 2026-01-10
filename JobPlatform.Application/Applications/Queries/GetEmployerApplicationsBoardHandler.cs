using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Applications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Queries;

public sealed class GetEmployerApplicationsBoardHandler
    : IRequestHandler<GetEmployerApplicationsBoardQuery, EmployerApplicationsBoardDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetEmployerApplicationsBoardHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EmployerApplicationsBoardDto> Handle(GetEmployerApplicationsBoardQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var limit = request.LimitPerStatus is < 1 ? 20 : request.LimitPerStatus;
        if (limit > 100) limit = 100;

        // Base query: all applications belong to employer
        var baseQ = _db.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(a => a.CandidateProfile)
            .Where(a => a.JobPost.EmployerProfile.UserId == userId)
            .AsQueryable();

        // counts per status (1 query)
        var countMap = await baseQ
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int CountOf(ApplicationStatus s) => countMap.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        // sort per column
        Func<IQueryable<JobApplication>, IQueryable<JobApplication>> sort = request.Sort?.ToLowerInvariant() switch
        {
            "applied_asc" => q => q.OrderBy(a => a.AppliedAt),
            "applied_desc" => q => q.OrderByDescending(a => a.AppliedAt),
            _ => q => q.OrderByDescending(a => a.StatusChangedAt ?? a.AppliedAt) // updated_desc default
        };

        async Task<List<EmployerBoardApplicationItemDto>> Load(ApplicationStatus status)
        {
            var q = baseQ.Where(a => a.Status == status);
            q = sort(q);

            // Query only needed fields
            var rows = await q.Take(limit)
                .Select(a => new
                {
                    a.Id,
                    a.JobPostId,
                    JobTitle = a.JobPost.Title,
                    a.CandidateProfileId,
                    CandidateName = a.CandidateProfile.FullName,
                    CandidateHeadline = a.CandidateProfile.Headline,
                    a.Status,
                    a.AppliedAt,
                    a.StatusChangedAt
                })
                .ToListAsync(ct);

            // add allowed transitions in-memory
            return rows.Select(r => new EmployerBoardApplicationItemDto(
                r.Id,
                r.JobPostId,
                r.JobTitle,
                r.CandidateProfileId,
                r.CandidateName,
                r.CandidateHeadline,
                r.Status,
                r.AppliedAt,
                r.StatusChangedAt,
                GetAllowedNextStatuses(r.Status)
            )).ToList();
        }

        var appliedItems = await Load(ApplicationStatus.Applied);
        var reviewedItems = await Load(ApplicationStatus.Reviewed);
        var interviewItems = await Load(ApplicationStatus.Interview);
        var offerItems = await Load(ApplicationStatus.Offer);
        var rejectedItems = await Load(ApplicationStatus.Rejected);

        return new EmployerApplicationsBoardDto(
            Applied: new BoardColumnDto(CountOf(ApplicationStatus.Applied), appliedItems),
            Reviewed: new BoardColumnDto(CountOf(ApplicationStatus.Reviewed), reviewedItems),
            Interview: new BoardColumnDto(CountOf(ApplicationStatus.Interview), interviewItems),
            Offer: new BoardColumnDto(CountOf(ApplicationStatus.Offer), offerItems),
            Rejected: new BoardColumnDto(CountOf(ApplicationStatus.Rejected), rejectedItems)
        );
    }

    private static List<ApplicationStatus> GetAllowedNextStatuses(ApplicationStatus from) => from switch
    {
        ApplicationStatus.Applied => new() { ApplicationStatus.Reviewed, ApplicationStatus.Rejected },
        ApplicationStatus.Reviewed => new() { ApplicationStatus.Interview, ApplicationStatus.Rejected },
        ApplicationStatus.Interview => new() { ApplicationStatus.Offer, ApplicationStatus.Rejected },
        _ => new() // Offer/Rejected: terminal => empty
    };
}
