using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Interviews.Commands;
using JobPlatform.Application.Interviews.DTOs;
using JobPlatform.Application.Interviews.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize(Policy = "EmployerOnly")]
public class EmployerInterviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EmployerInterviewsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("/employer/interviews")]
    public async Task<ActionResult<PagedResult<EmployerInterviewListItemDto>>> List(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] bool includeCancelled = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => Ok(await _mediator.Send(new GetEmployerInterviewsQuery(from, to, includeCancelled, page, pageSize)));
    
    [HttpGet("/employer/interviews/{id:guid}")]
    public async Task<ActionResult<InterviewDetailDto>> Detail(Guid id)
        => Ok(await _mediator.Send(new GetEmployerInterviewDetailQuery(id)));
    
    [HttpPost("/employer/applications/{applicationId:guid}/interviews")]
    public async Task<IActionResult> Create(Guid applicationId, [FromBody] CreateInterviewDto dto)
    {
        var id = await _mediator.Send(new CreateInterviewCommand(applicationId, dto));
        return Ok(new { interviewId = id });
    }

    [HttpPut("/employer/interviews/{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleInterviewDto dto)
    {
        await _mediator.Send(new RescheduleInterviewCommand(id, dto));
        return NoContent();
    }

    [HttpPut("/employer/interviews/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelInterviewBody body)
    {
        await _mediator.Send(new CancelInterviewCommand(id, body.Reason));
        return NoContent();
    }

    public sealed record CancelInterviewBody(string? Reason);
}