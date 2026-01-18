using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Interviews.Commands;
using JobPlatform.Application.Interviews.DTOs;
using JobPlatform.Application.Interviews.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize(Policy = "CandidateOnly")]
public class CandidateInterviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CandidateInterviewsController(IMediator mediator) => _mediator = mediator;

    
    [HttpGet("/candidates/me/interviews")]
    public async Task<ActionResult<PagedResult<CandidateInterviewListItemDto>>> List(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] bool upcomingOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetMyInterviewsQuery(from, to, upcomingOnly, page, pageSize)));

    [HttpGet("/candidates/me/interviews/{id:guid}")]
    public async Task<ActionResult<InterviewDetailDto>> Detail(Guid id)
        => Ok(await _mediator.Send(new GetCandidateInterviewDetailQuery(id)));
    
    [HttpPut("/candidates/me/interviews/{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        await _mediator.Send(new ConfirmInterviewCommand(id));
        return NoContent();
    }

    [HttpPut("/candidates/me/interviews/{id:guid}/request-reschedule")]
    public async Task<IActionResult> RequestReschedule(Guid id, [FromBody] RequestRescheduleDto dto)
    {
        await _mediator.Send(new RequestRescheduleCommand(id, dto));
        return NoContent();
    }
}