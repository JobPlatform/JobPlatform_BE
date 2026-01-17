using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Notifications.DTOs;
using MediatR;

namespace JobPlatform.Application.Notifications.Queries;

public sealed record GetMyNotificationsQuery(
    bool? IsRead,
    string? Type,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<NotificationListItemDto>>;