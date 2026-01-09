using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Jobs.DTOs;
using MediatR;

namespace JobPlatform.Application.Jobs.Commands;

public sealed record UpdateJobDraftCommand(Guid JobId, UpdateJobDraftDto Dto) : IRequest<JobDetailDto>,ITransactionalRequest;