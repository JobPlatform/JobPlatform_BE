using JobPlatform.Application.Applications.DTOs;
using MediatR;

namespace JobPlatform.Application.Applications.Queries;

public sealed record GetEmployerApplicationsBoardQuery(
    int LimitPerStatus = 20,
    string? Sort = "updated_desc"
) : IRequest<EmployerApplicationsBoardDto>;