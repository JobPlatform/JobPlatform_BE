using JobPlatform.Application.Jobs.DTOs;
using MediatR;

namespace JobPlatform.Application.Jobs.Commands;

public sealed record PublishJobCommand(Guid JobId) : IRequest<JobDetailDto>;