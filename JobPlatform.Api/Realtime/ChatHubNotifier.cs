using JobPlatform.Api.Hubs;
using JobPlatform.Application.Chat.DTOs;
using JobPlatform.Application.Common.Interfaces;

using Microsoft.AspNetCore.SignalR;

namespace JobPlatform.Api.Realtime;

public sealed class ChatHubNotifier : IChatHubNotifier
{
    private readonly IHubContext<ChatHub> _hub;
    public ChatHubNotifier(IHubContext<ChatHub> hub) => _hub = hub;

    public Task PushNewMessageAsync(Guid jobApplicationId, MessageDto message, CancellationToken ct)
        => _hub.Clients.Group(ChatHub.AppGroup(jobApplicationId))
            .SendAsync("message:new", message, ct);

    public Task PushUnreadDeltaAsync(Guid receiverUserId, Guid jobApplicationId, int delta, CancellationToken ct)
        => _hub.Clients.Group(ChatHub.UserGroup(receiverUserId))
            .SendAsync("unread:delta", new { jobApplicationId, delta }, ct);

    public Task PushReadUpdatedAsync(Guid jobApplicationId, Guid readerUserId, Guid lastMessageId, DateTimeOffset lastReadAt, CancellationToken ct)
        => _hub.Clients.Group(ChatHub.AppGroup(jobApplicationId))
            .SendAsync("read:updated", new { readerUserId, lastMessageId, lastReadAt }, ct);
}