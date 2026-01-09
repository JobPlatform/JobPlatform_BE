using JobPlatform.Application.Skills.DTOs;
using JobPlatform.Application.Skills.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("skills")]
public class SkillsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SkillsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("domains")]
    public async Task<ActionResult<List<SkillDomainDto>>> GetDomains()
        => Ok(await _mediator.Send(new GetSkillDomainsQuery()));

    [HttpGet("categories")]
    public async Task<ActionResult<List<SkillCategoryDto>>> GetCategories(
        [FromQuery] string? domainCode,
        [FromQuery] Guid? domainId)
        => Ok(await _mediator.Send(new GetSkillCategoriesQuery(domainCode, domainId)));

    [HttpGet]
    public async Task<ActionResult<List<SkillDto>>> GetSkills(
        [FromQuery] string? domainCode,
        [FromQuery] string? categoryCode,
        [FromQuery] string? q,
        [FromQuery] bool activeOnly = true)
        => Ok(await _mediator.Send(new GetSkillsQuery(domainCode, categoryCode, q, activeOnly)));
}