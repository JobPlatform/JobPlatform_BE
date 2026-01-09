using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Application.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("jobs")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    public JobsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<JobListItemDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? location,
        [FromQuery] int? workMode,
        [FromQuery] decimal? salaryMin,
        [FromQuery] decimal? salaryMax,
        [FromQuery] string? skills,         // csv: "c#,sql,docker"
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var skillList = string.IsNullOrWhiteSpace(skills)
            ? null
            : skills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

        var result = await _mediator.Send(new SearchJobsQuery(
            Q: q,
            Location: location,
            WorkMode: workMode,
            SalaryMin: salaryMin,
            SalaryMax: salaryMax,
            Skills: skillList,
            Sort: sort,
            Page: page,
            PageSize: pageSize
        ));

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> Get(Guid id)
        => Ok(await _mediator.Send(new GetPublishedJobByIdQuery(id)));
}