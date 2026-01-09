using JobPlatform.Application.Jobs.Commands;
using JobPlatform.Application.Jobs.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("employer/jobs")]
[Authorize(Policy = "EmployerOnly")]
public class EmployerJobsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EmployerJobsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<JobDetailDto>> CreateDraft([FromBody] CreateJobDraftDto dto)
        => Ok(await _mediator.Send(new CreateJobDraftCommand(dto)));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> UpdateDraft(Guid id, [FromBody] UpdateJobDraftDto dto)
        => Ok(await _mediator.Send(new UpdateJobDraftCommand(id, dto)));

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<JobDetailDto>> Publish(Guid id)
        => Ok(await _mediator.Send(new PublishJobCommand(id)));
}