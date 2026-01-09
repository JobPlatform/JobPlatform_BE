using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JobPlatform.Application.Common.Auth;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JobPlatform.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public TokenService(AppDbContext db, UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _db = db;
        _userManager = userManager;
        _config = config;
    }

    public async Task<(string accessToken, string refreshToken)> CreateTokensAsync(
        TokenSubjectDto subject,
        CancellationToken ct = default)
    {
        var (accessToken, refreshToken, refreshEntity) = CreateTokenPair(subject);

        _db.RefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync(ct);

        return (accessToken, refreshToken);
    }

    public async Task<(string accessToken, string refreshToken)?> RefreshAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);

        var existing = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (existing is null || !existing.IsActive)
            return null;

        // revoke old token
        existing.RevokedAt = DateTimeOffset.UtcNow;

        // load user (safety fallback if navigation is null)
        var user = existing.User;
        if (user is null)
        {
            user = await _userManager.FindByIdAsync(existing.UserId.ToString());
            if (user is null) return null;
        }

        // build subject from user + roles
        var roles = await _userManager.GetRolesAsync(user);
        var subject = new TokenSubjectDto(
            UserId: user.Id,
            Email: user.Email,
            UserName: user.UserName ?? user.Email ?? user.Id.ToString(),
            Roles: roles.ToArray()
        );

        // create new pair (rotate)
        var (accessToken, newRefreshToken, newRefreshEntity) = CreateTokenPair(subject);
        _db.RefreshTokens.Add(newRefreshEntity);

        // save once
        await _db.SaveChangesAsync(ct);

        return (accessToken, newRefreshToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = Hash(refreshToken);

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (token is null) return;

        token.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private (string accessToken, string refreshToken, RefreshToken refreshEntity) CreateTokenPair(TokenSubjectDto subject)
    {
        var jwt = _config.GetSection("Jwt");

        var keyStr = jwt["Key"];
        if (string.IsNullOrWhiteSpace(keyStr))
            throw new InvalidOperationException("Missing configuration: Jwt:Key");

        if (!int.TryParse(jwt["AccessTokenMinutes"], out var accessMinutes))
            throw new InvalidOperationException("Invalid configuration: Jwt:AccessTokenMinutes");

        if (!int.TryParse(jwt["RefreshTokenDays"], out var refreshDays))
            throw new InvalidOperationException("Invalid configuration: Jwt:RefreshTokenDays");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, subject.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, subject.UserId.ToString()),
            new(ClaimTypes.Name, subject.UserName),
        };

        if (subject.Roles is { Count: > 0 })
            claims.AddRange(subject.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessMinutes),
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // refresh token random + store hash
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshHash = Hash(refreshToken);

        var refreshEntity = new RefreshToken
        {
            UserId = subject.UserId,
            TokenHash = refreshHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshDays),
        };

        return (accessToken, refreshToken, refreshEntity);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
