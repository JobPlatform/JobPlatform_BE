using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Chat.Commands;

public sealed class MarkChatReadHandler : IRequestHandler<MarkChatReadCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IChatHubNotifier _hub;

    public MarkChatReadHandler(IAppDbContext db, ICurrentUserService current, IChatHubNotifier hub)
    {
        _db = db; _current = current; _hub = hub;
    }

    public async Task<Unit> Handle(MarkChatReadCommand request, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new ForbiddenException("Unauthorized");

        var appUsers = await _db.JobApplications.AsNoTracking()
            .Where(a => a.Id == request.JobApplicationId)
            .Select(a => new { a.CandidateProfile.UserId, EmployerUserId = a.JobPost.EmployerProfile.UserId })
            .FirstOrDefaultAsync(ct);

        if (appUsers is null) throw new NotFoundException("Application not found");
        if (appUsers.UserId != userId && appUsers.EmployerUserId != userId) throw new ForbiddenException();

        var convId = request.JobApplicationId;

        Guid? lastId = request.LastMessageId;
        DateTimeOffset? lastAt = null;

        if (lastId is null)
        {
            var last = await _db.Messages.AsNoTracking()
                .Where(m => m.ConversationId == convId)
                .OrderByDescending(m => m.SentAt).ThenByDescending(m => m.Id)
                .Select(m => new { m.Id, m.SentAt })
                .FirstOrDefaultAsync(ct);

            if (last is null) return Unit.Value;
            lastId = last.Id;
            lastAt = last.SentAt;
        }
        else
        {
            lastAt = await _db.Messages.AsNoTracking()
                .Where(m => m.ConversationId == convId && m.Id == lastId)
                .Select(m => (DateTimeOffset?)m.SentAt)
                .FirstOrDefaultAsync(ct);

            if (lastAt is null) throw new BadRequestException("Invalid LastMessageId");
        }

        await _db.ConversationMembers
            .Where(m => m.ConversationId == convId && m.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.UnreadCount, 0)
                .SetProperty(x => x.LastReadMessageId, lastId)
                .SetProperty(x => x.LastReadAt, lastAt), ct);

        
        await _hub.PushReadUpdatedAsync(request.JobApplicationId, userId, lastId!.Value, lastAt!.Value, ct);
        return Unit.Value;
    }
}
