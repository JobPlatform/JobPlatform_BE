namespace JobPlatform.Application.Common.Interfaces;

public interface IInterviewReminderJob
{
    Task RunAsync(CancellationToken ct);
}