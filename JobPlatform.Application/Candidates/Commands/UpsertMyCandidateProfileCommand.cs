using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Application.Common.Interfaces;
using MediatR;

namespace JobPlatform.Application.Candidates.Commands;

public sealed record UpsertMyCandidateProfileCommand(UpdateCandidateProfileDto Dto) : IRequest<CandidateProfileDto>, ITransactionalRequest;