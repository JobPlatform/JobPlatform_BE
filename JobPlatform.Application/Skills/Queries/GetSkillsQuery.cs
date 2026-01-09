using JobPlatform.Application.Skills.DTOs;
using MediatR;

namespace JobPlatform.Application.Skills.Queries;

public sealed record GetSkillsQuery(
    string? DomainCode,
    string? CategoryCode,
    string? Q,
    bool ActiveOnly = true
) : IRequest<List<SkillDto>>;