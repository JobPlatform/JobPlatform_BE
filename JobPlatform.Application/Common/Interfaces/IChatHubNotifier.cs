using JobPlatform.Application.Chat.DTOs;

namespace JobPlatform.Application.Common.Interfaces;

public interface IChatHubNotifier
{
    Task PushNewMessageAsync(Guid jobApplicationId, MessageDto message, CancellationToken ct);
    Task PushUnreadDeltaAsync(Guid receiverUserId, Guid jobApplicationId, int delta, CancellationToken ct);
    Task PushReadUpdatedAsync(Guid jobApplicationId, Guid readerUserId, Guid lastMessageId, DateTimeOffset lastReadAt, CancellationToken ct);
}