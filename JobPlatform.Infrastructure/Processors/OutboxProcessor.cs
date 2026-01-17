using JobPlatform.Infrastructure.Persistence;

namespace JobPlatform.Infrastructure.Processors;

using System.Text.Json;
using JobPlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly AppDbContext _db;
    private readonly IJobMatchingService _matching;

    public OutboxProcessor(AppDbContext db, IJobMatchingService matching)
    {
        _db = db;
        _matching = matching;
    }

    public async Task ProcessAsync(CancellationToken ct)
    {
        
        var batch = await _db.OutboxMessages
            .Where(x => x.ProcessedAt == null && x.Attempt < 10)
            .OrderBy(x => x.OccurredAt)
            .Take(20)
            .ToListAsync(ct);

        foreach (var msg in batch)
        {
            try
            {
                if (msg.Type == "JobPublished")
                {
                    var payload = JsonSerializer.Deserialize<JobPublishedPayload>(msg.PayloadJson)
                                  ?? throw new Exception("Invalid payload");
                    await _matching.MatchForJobAsync(payload.JobId, ct);
                }

                msg.ProcessedAt = DateTimeOffset.UtcNow;
                msg.Error = null;
            }
            catch (Exception ex)
            {
                msg.Attempt += 1;
                msg.Error = ex.Message;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private sealed record JobPublishedPayload(Guid JobId, Guid EmployerUserId);
}
