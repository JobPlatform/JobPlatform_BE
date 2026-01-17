namespace JobPlatform.Application.Common.Interfaces;

public interface IJobMatchingService
{
    Task MatchForJobAsync(Guid jobId, CancellationToken ct);
}
