using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Notifications.Commands;

public sealed class MarkAllMyNotificationsReadHandler : IRequestHandler<MarkAllMyNotificationsReadCommand, int>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public MarkAllMyNotificationsReadHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(MarkAllMyNotificationsReadCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (unread.Count == 0) return 0;

        var now = DateTimeOffset.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);
        return unread.Count;
    }
}