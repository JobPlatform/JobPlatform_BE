namespace JobPlatform.Application.Applications.DTOs;

public sealed record ChangeApplicationStatusDto(
    int Status,         // map enum
    string? Note
);