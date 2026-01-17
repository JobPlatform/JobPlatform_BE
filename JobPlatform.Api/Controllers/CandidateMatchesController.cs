using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Matches.DTOs;
using JobPlatform.Application.Matches.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("candidates/me/matches")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateMatchesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CandidateMatchesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CandidateJobMatchDto>>> List(
        [FromQuery] Guid? jobId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetMyMatchesQuery(jobId, page, pageSize)));
}