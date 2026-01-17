namespace JobPlatform.Application.Common.Interfaces;

public interface IOutboxProcessor
{
    Task ProcessAsync(CancellationToken ct);
}
