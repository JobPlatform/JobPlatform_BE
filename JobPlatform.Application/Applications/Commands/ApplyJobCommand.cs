using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Interfaces;
using MediatR;

namespace JobPlatform.Application.Applications.Commands;

public sealed record ApplyJobCommand(Guid JobId, ApplyJobDto Dto) : IRequest<Guid>,ITransactionalRequest; // return ApplicationId