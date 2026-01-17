using MediatR;

namespace JobPlatform.Application.Notifications.Commands;

public sealed record MarkAllMyNotificationsReadCommand() : IRequest<int>; // returns updated count