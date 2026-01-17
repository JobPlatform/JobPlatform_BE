using MediatR;

namespace JobPlatform.Application.Notifications.Queries;

public sealed record GetMyUnreadCountQuery() : IRequest<int>;