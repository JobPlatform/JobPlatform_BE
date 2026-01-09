using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Application.Common.Interfaces;
using MediatR;

namespace JobPlatform.Application.Candidates.Queries;

public sealed record GetMyCandidateProfileQuery() : IRequest<CandidateProfileDto>,ITransactionalRequest;