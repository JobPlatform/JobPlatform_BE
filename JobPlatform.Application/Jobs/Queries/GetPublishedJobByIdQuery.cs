using JobPlatform.Application.Jobs.DTOs;
using MediatR;

namespace JobPlatform.Application.Jobs.Queries;

public sealed record GetPublishedJobByIdQuery(Guid JobId) : IRequest<JobDetailDto>;