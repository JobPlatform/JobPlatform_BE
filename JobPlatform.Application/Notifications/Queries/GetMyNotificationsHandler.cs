using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Notifications.Queries;

public sealed class GetMyNotificationsHandler
    : IRequestHandler<GetMyNotificationsQuery, PagedResult<NotificationListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<NotificationListItemDto>> Handle(GetMyNotificationsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var q = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .AsQueryable();

        if (request.IsRead.HasValue)
            q = q.Where(n => n.IsRead == request.IsRead.Value);

        if (!string.IsNullOrWhiteSpace(request.Type))
            q = q.Where(n => n.Type == request.Type.Trim());

        q = q.OrderByDescending(n => n.CreatedAt);

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationListItemDto(
                n.Id,
                n.Type,
                n.Title,
                n.Body,
                n.DataJson,
                n.TargetUrl,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<NotificationListItemDto>(items, page, pageSize, total);
    }
}