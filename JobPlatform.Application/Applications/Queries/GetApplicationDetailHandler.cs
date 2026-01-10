using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Queries;

public sealed class GetApplicationDetailHandler : IRequestHandler<GetApplicationDetailQuery, ApplicationDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetApplicationDetailHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ApplicationDetailDto> Handle(GetApplicationDetailQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var app = await _db.JobApplications
            .Include(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct);

        if (app is null) throw new NotFoundException("Application not found");
        if (app.JobPost.EmployerProfile.UserId != userId) throw new ForbiddenException();

        return new ApplicationDetailDto(
            app.Id,
            app.JobPostId,
            app.JobPost.Title,
            app.CandidateProfileId,
            app.CandidateProfile.FullName,
            app.CandidateProfile.Headline,
            app.CandidateProfile.Location,
            app.CandidateProfile.CvStoragePath,
            app.Status,
            app.CoverLetter,
            app.StatusNote,
            app.AppliedAt,
            app.StatusChangedAt
        );
    }
}