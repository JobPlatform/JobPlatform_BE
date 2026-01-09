using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Application.Common.Interfaces;
using MediatR;

namespace JobPlatform.Application.Candidates.Commands;

public sealed record UpdateMyCandidateSkillsCommand(UpdateCandidateSkillsDto Dto) : IRequest<CandidateProfileDto>, ITransactionalRequest;