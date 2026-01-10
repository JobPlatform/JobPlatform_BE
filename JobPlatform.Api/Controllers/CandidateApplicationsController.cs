using JobPlatform.Application.Applications.Commands;
using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Applications.Queries;
using JobPlatform.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("candidates/me/applications")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CandidateApplicationsController(IMediator mediator) => _mediator = mediator;

    
    [HttpGet]
    public async Task<ActionResult<PagedResult<MyApplicationListItemDto>>> MyApplications(
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetMyApplicationsQuery(status, page, pageSize)));

    
    [HttpPost("/jobs/{jobId:guid}/apply")]
    public async Task<IActionResult> Apply(Guid jobId, [FromBody] ApplyJobDto dto)
    {
        var appId = await _mediator.Send(new ApplyJobCommand(jobId, dto));
        return Ok(new { applicationId = appId });
    }
}