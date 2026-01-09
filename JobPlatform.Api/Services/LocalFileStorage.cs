using JobPlatform.Application.Common.Interfaces;

namespace JobPlatform.Api.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorage(IWebHostEnvironment env) => _env = env;

    public async Task<(string storagePath, string fileName, string contentType, long size)> SaveCandidateCvAsync(
        Guid userId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct)
    {
        var safeFileName = SanitizeFileName(fileName);
        var ext = Path.GetExtension(safeFileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".pdf";

        var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "cv", userId.ToString());
        Directory.CreateDirectory(folder);

        var finalFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, finalFileName);

        await using var outStream = File.Create(fullPath);
        await stream.CopyToAsync(outStream, ct);

        var relative = $"/uploads/cv/{userId}/{finalFileName}";
        var fi = new FileInfo(fullPath);

        return (relative, safeFileName, contentType, fi.Length);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Where(ch => !invalid.Contains(ch)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "cv.pdf" : cleaned;
    }
}