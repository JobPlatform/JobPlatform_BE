using JobPlatform.Application.Chat.Commands;
using JobPlatform.Application.Chat.DTOs;
using JobPlatform.Application.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Authorize]
[Route("job-applications/{applicationId:guid}/chat")]
public class JobApplicationChatController : ControllerBase
{
    private readonly IMediator _mediator;
    public JobApplicationChatController(IMediator mediator) => _mediator = mediator;

    // GET /job-applications/{id}/chat
    [HttpGet]
    public async Task<ActionResult<ChatSummaryDto>> Summary(Guid applicationId)
        => Ok(await _mediator.Send(new GetOrCreateChatSummaryQuery(applicationId)));

    // GET /job-applications/{id}/chat/messages?cursorSentAt=...&cursorId=...&pageSize=50
    [HttpGet("messages")]
    public async Task<ActionResult<CursorPagedResult<MessageDto>>> Messages(
        Guid applicationId,
        [FromQuery] DateTimeOffset? cursorSentAt,
        [FromQuery] Guid? cursorId,
        [FromQuery] int pageSize = 50)
        => Ok(await _mediator.Send(new GetChatMessagesQuery(applicationId, cursorSentAt, cursorId, pageSize)));

    // POST /job-applications/{id}/chat/messages
    [HttpPost("messages")]
    public async Task<ActionResult<MessageDto>> Send(Guid applicationId, [FromBody] SendMessageBody body)
        => Ok(await _mediator.Send(new SendChatMessageCommand(applicationId, body.Content)));

    // PUT /job-applications/{id}/chat/read
    [HttpPut("read")]
    public async Task<IActionResult> MarkRead(Guid applicationId, [FromBody] MarkReadBody body)
    {
        await _mediator.Send(new MarkChatReadCommand(applicationId, body.LastMessageId));
        return NoContent();
    }

    public sealed record SendMessageBody(string Content);
    public sealed record MarkReadBody(Guid? LastMessageId);
}