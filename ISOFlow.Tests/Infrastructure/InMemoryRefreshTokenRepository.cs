using System.Collections.Concurrent;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Tests.Infrastructure;

/// <summary>In-memory <see cref="IRefreshTokenRepository"/> test double for unit-testing AuthService flows.</summary>
public sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
        Task.FromResult(_tokens.TryGetValue(tokenHash, out var token) ? token : null);

    public Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken.Id))
            refreshToken.Id = (_tokens.Count + 1).ToString();
        _tokens[refreshToken.TokenHash] = refreshToken;
        return Task.FromResult(refreshToken);
    }

    public Task<bool> UpdateAsync(RefreshToken refreshToken)
    {
        if (!_tokens.ContainsKey(refreshToken.TokenHash)) return Task.FromResult(false);
        _tokens[refreshToken.TokenHash] = refreshToken;
        return Task.FromResult(true);
    }

    public Task<bool> RevokeByTokenHashAsync(string tokenHash, string? replacedByTokenHash = null)
    {
        if (!_tokens.TryGetValue(tokenHash, out var token) || token.RevokedAt != null)
            return Task.FromResult(false);

        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByTokenHash = replacedByTokenHash;
        return Task.FromResult(true);
    }

    public Task<bool> RevokeAllForUserAsync(string userId)
    {
        var active = _tokens.Values.Where(t => t.UserId == userId && t.RevokedAt == null).ToList();
        foreach (var token in active)
            token.RevokedAt = DateTime.UtcNow;
        return Task.FromResult(active.Count > 0);
    }
}
