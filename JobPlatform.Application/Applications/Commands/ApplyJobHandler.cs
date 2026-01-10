using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Applications;
using JobPlatform.Domain.Entities.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Commands;

public sealed class ApplyJobHandler : IRequestHandler<ApplyJobCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ApplyJobHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(ApplyJobCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var candidate = await _db.CandidateProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (candidate is null)
            throw new NotFoundException("Candidate profile not found");

        var job = await _db.JobPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.JobId, ct);

        if (job is null) throw new NotFoundException("Job not found");
        if (job.Status != JobStatus.Published)
            throw new BadRequestException("You can only apply to Published jobs");

        // prevent duplicate
        var exists = await _db.JobApplications
            .AnyAsync(a => a.JobPostId == request.JobId && a.CandidateProfileId == candidate.Id, ct);

        if (exists)
            throw new BadRequestException("You already applied to this job");

        var app = new JobApplication
        {
            JobPostId = request.JobId,
            CandidateProfileId = candidate.Id,
            CoverLetter = string.IsNullOrWhiteSpace(request.Dto.CoverLetter) ? null : request.Dto.CoverLetter.Trim(),
            Status = ApplicationStatus.Applied,
            AppliedAt = DateTimeOffset.UtcNow
        };

        _db.JobApplications.Add(app);
        await _db.SaveChangesAsync(ct);

        return app.Id;
    }
}
