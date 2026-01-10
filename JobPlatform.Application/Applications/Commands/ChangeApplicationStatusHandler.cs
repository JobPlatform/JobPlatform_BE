using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Applications.Commands;

public sealed class ChangeApplicationStatusHandler : IRequestHandler<ChangeApplicationStatusCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ChangeApplicationStatusHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(ChangeApplicationStatusCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        if (!Enum.IsDefined(typeof(ApplicationStatus), request.Dto.Status))
            throw new BadRequestException("Invalid status");

        var newStatus = (ApplicationStatus)request.Dto.Status;

        var app = await _db.JobApplications
            .Include(a => a.JobPost).ThenInclude(j => j.EmployerProfile)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct);

        if (app is null) throw new NotFoundException("Application not found");
        if (app.JobPost.EmployerProfile.UserId != userId) throw new ForbiddenException();

        var allowed = GetAllowedNextStatuses(app.Status);

        if (app.Status is ApplicationStatus.Offer or ApplicationStatus.Rejected)
            throw new BadRequestException($"Cannot change status from {app.Status} because it is terminal.");

        if (!allowed.Contains(newStatus) && newStatus != app.Status)
            throw new BadRequestException(
                $"Cannot change status from {app.Status} to {newStatus}. Allowed: {string.Join(", ", allowed)}"
            );


        if (newStatus == ApplicationStatus.Rejected && string.IsNullOrWhiteSpace(request.Dto.Note))
            throw new BadRequestException("Note is required when rejecting an application.");

        app.Status = newStatus;
        app.StatusChangedAt = DateTimeOffset.UtcNow;
        app.StatusNote = string.IsNullOrWhiteSpace(request.Dto.Note) ? null : request.Dto.Note.Trim();
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    
    private static List<ApplicationStatus> GetAllowedNextStatuses(ApplicationStatus from) => from switch
    {
        ApplicationStatus.Applied => new() { ApplicationStatus.Reviewed, ApplicationStatus.Rejected },
        ApplicationStatus.Reviewed => new() { ApplicationStatus.Interview, ApplicationStatus.Rejected },
        ApplicationStatus.Interview => new() { ApplicationStatus.Offer, ApplicationStatus.Rejected },
        _ => new()
    };
    
    private static bool IsAllowedTransition(ApplicationStatus from, ApplicationStatus to)
    {
        if (from == to) return true;

        return from switch
        {
            ApplicationStatus.Applied => to is ApplicationStatus.Reviewed or ApplicationStatus.Rejected,
            ApplicationStatus.Reviewed => to is ApplicationStatus.Interview or ApplicationStatus.Rejected,
            ApplicationStatus.Interview => to is ApplicationStatus.Offer or ApplicationStatus.Rejected,
            ApplicationStatus.Offer => false,      // terminal
            ApplicationStatus.Rejected => false,   // terminal
            _ => false
        };
    }
}
