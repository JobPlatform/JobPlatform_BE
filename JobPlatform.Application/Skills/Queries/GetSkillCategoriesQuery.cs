using JobPlatform.Application.Skills.DTOs;
using MediatR;

namespace JobPlatform.Application.Skills.Queries;

public sealed record GetSkillCategoriesQuery(string? DomainCode, Guid? DomainId) : IRequest<List<SkillCategoryDto>>;