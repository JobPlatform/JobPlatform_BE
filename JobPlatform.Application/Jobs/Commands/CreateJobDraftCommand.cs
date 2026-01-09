using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Jobs.DTOs;
using MediatR;

namespace JobPlatform.Application.Jobs.Commands;

public sealed record CreateJobDraftCommand(CreateJobDraftDto Dto) : IRequest<JobDetailDto>, ITransactionalRequest;