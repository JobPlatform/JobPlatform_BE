using JobPlatform.Application.Common.Auth;

namespace JobPlatform.Application.Common.Interfaces;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken)> CreateTokensAsync(
        TokenSubjectDto subject,
        CancellationToken ct = default);

    Task<(string accessToken, string refreshToken)?> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default);

    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}