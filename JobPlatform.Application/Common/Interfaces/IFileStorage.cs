namespace JobPlatform.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<(string storagePath, string fileName, string contentType, long size)> SaveCandidateCvAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct);
}