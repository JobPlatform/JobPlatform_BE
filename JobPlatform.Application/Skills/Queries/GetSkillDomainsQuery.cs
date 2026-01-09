using JobPlatform.Application.Skills.DTOs;
using MediatR;

namespace JobPlatform.Application.Skills.Queries;

public sealed record GetSkillDomainsQuery() : IRequest<List<SkillDomainDto>>;