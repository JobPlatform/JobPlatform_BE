using JobPlatform.Application.Applications.Commands;
using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Applications.Queries;
using JobPlatform.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("employer/applications")]
[Authorize(Policy = "EmployerOnly")]
public class EmployerApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EmployerApplicationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("/employer/jobs/{jobId:guid}/applications")]
    public async Task<ActionResult<PagedResult<ApplicationListItemDto>>> ListByJob(
        Guid jobId,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetApplicationsByJobQuery(jobId, status, page, pageSize)));

    [HttpGet("{applicationId:guid}")]
    public async Task<ActionResult<ApplicationDetailDto>> Detail(Guid applicationId)
        => Ok(await _mediator.Send(new GetApplicationDetailQuery(applicationId)));

    
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployerApplicationKanbanItemDto>>> Kanban(
        [FromQuery] int? status,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetEmployerApplicationsQuery(status, sort, page, pageSize)));

    
    [HttpGet("board")]
    public async Task<ActionResult<EmployerApplicationsBoardDto>> Board(
        [FromQuery] int limitPerStatus = 20,
        [FromQuery] string? sort = "updated_desc")
        => Ok(await _mediator.Send(new GetEmployerApplicationsBoardQuery(limitPerStatus, sort)));

    
    [HttpPut("{applicationId:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid applicationId, [FromBody] ChangeApplicationStatusDto dto)
    {
        await _mediator.Send(new ChangeApplicationStatusCommand(applicationId, dto));
        return NoContent();
    }
}