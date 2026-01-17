using MediatR;

namespace JobPlatform.Application.Notifications.Commands;

public sealed record MarkMyNotificationReadCommand(Guid NotificationId) : IRequest;