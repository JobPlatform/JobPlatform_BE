using JobPlatform.Application.Applications.DTOs;
using MediatR;

namespace JobPlatform.Application.Applications.Queries;

public sealed record GetApplicationDetailQuery(Guid ApplicationId) : IRequest<ApplicationDetailDto>;