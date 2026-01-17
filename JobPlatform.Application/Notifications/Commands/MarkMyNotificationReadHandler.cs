using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Notifications.Commands;

public sealed class MarkMyNotificationReadHandler : IRequestHandler<MarkMyNotificationReadCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkMyNotificationReadHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(MarkMyNotificationReadCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var noti = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == request.NotificationId, ct);
        if (noti is null) throw new NotFoundException("Notification not found");

        if (noti.UserId != userId) throw new ForbiddenException();

        if (!noti.IsRead)
        {
            noti.IsRead = true;
            noti.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        
        return Unit.Value;
    }
}