using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Notifications.Commands;
using JobPlatform.Application.Notifications.DTOs;
using JobPlatform.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("notifications")]
[Authorize] // mọi user đăng nhập đều dùng được
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    // GET /notifications?isRead=false&type=JobApplied&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationListItemDto>>> List(
        [FromQuery] bool? isRead,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetMyNotificationsQuery(isRead, type, page, pageSize)));

    // GET /notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
        => Ok(new { count = await _mediator.Send(new GetMyUnreadCountQuery()) });

    // PUT /notifications/{id}/read
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await _mediator.Send(new MarkMyNotificationReadCommand(id));
        return NoContent();
    }

    // PUT /notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var updated = await _mediator.Send(new MarkAllMyNotificationsReadCommand());
        return Ok(new { updated });
    }
}