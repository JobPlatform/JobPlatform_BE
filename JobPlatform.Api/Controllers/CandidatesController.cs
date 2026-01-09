using JobPlatform.Application.Candidates.Commands;
using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Application.Candidates.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("candidates/me")]
[Authorize(Policy = "CandidateOnly")]
public class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CandidatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<CandidateProfileDto>> GetMe()
        => Ok(await _mediator.Send(new GetMyCandidateProfileQuery()));

    [HttpPut]
    public async Task<ActionResult<CandidateProfileDto>> UpsertProfile([FromBody] UpdateCandidateProfileDto dto)
        => Ok(await _mediator.Send(new UpsertMyCandidateProfileCommand(dto)));

    [HttpPut("skills")]
    public async Task<ActionResult<CandidateProfileDto>> UpdateSkills([FromBody] UpdateCandidateSkillsDto dto)
        => Ok(await _mediator.Send(new UpdateMyCandidateSkillsCommand(dto)));

    // Upload CV: multipart/form-data
    [HttpPost("cv")]
    [RequestSizeLimit(10_000_000)] // 10MB
    public async Task<ActionResult<CandidateProfileDto>> UploadCv([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required" });
    
        await using var stream = file.OpenReadStream();
        return Ok(await _mediator.Send(new UploadMyCvCommand(stream, file.FileName, file.ContentType), ct));
    }
}