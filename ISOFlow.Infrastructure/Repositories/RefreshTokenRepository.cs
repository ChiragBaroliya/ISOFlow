using Dapper;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL-backed refresh token store. Database errors propagate to the caller: a token that could
/// not be persisted, or a revocation that could not be written, must never be reported as successful.
/// </summary>
public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
{
    public RefreshTokenRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return QueryMappedFirstOrDefaultAsync(
            @"SELECT id, user_id, token_hash, created_at, expires_at, revoked_at, replaced_by_token_hash
              FROM refresh_tokens
              WHERE token_hash = @tokenHash",
            r => new RefreshToken
            {
                Id = r.id.ToString(),
                UserId = r.user_id.ToString(),
                TokenHash = (string)r.token_hash,
                CreatedAt = (DateTime)r.created_at,
                ExpiresAt = (DateTime)r.expires_at,
                RevokedAt = (DateTime?)r.revoked_at,
                ReplacedByTokenHash = (string?)r.replaced_by_token_hash
            },
            new { tokenHash });
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        if (!int.TryParse(refreshToken.UserId, out var userId))
            throw new ArgumentException($"Refresh token user id '{refreshToken.UserId}' is not a valid user id.", nameof(refreshToken));

        var parameters = new DynamicParameters();
        parameters.Add("p_user_id", userId);
        parameters.Add("p_token_hash", refreshToken.TokenHash);
        parameters.Add("p_created_at", refreshToken.CreatedAt);
        parameters.Add("p_expires_at", refreshToken.ExpiresAt);
        parameters.Add("p_revoked_at", refreshToken.RevokedAt);
        parameters.Add("p_replaced_by", refreshToken.ReplacedByTokenHash);

        var id = await QuerySingleAsync<int>(
            @"INSERT INTO refresh_tokens (user_id, token_hash, created_at, expires_at, revoked_at, replaced_by_token_hash)
              VALUES (@p_user_id, @p_token_hash, @p_created_at, @p_expires_at, @p_revoked_at, @p_replaced_by)
              RETURNING id",
            parameters);

        refreshToken.Id = id.ToString();
        return refreshToken;
    }

    public async Task<bool> UpdateAsync(RefreshToken refreshToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_token_hash", refreshToken.TokenHash);
        parameters.Add("p_revoked_at", refreshToken.RevokedAt);
        parameters.Add("p_replaced_by", refreshToken.ReplacedByTokenHash);

        var rows = await ExecuteAsync(
            @"UPDATE refresh_tokens
              SET revoked_at = @p_revoked_at, replaced_by_token_hash = @p_replaced_by
              WHERE token_hash = @p_token_hash",
            parameters);

        return rows > 0;
    }

    public async Task<bool> RevokeByTokenHashAsync(string tokenHash, string? replacedByTokenHash = null)
    {
        var rows = await ExecuteAsync(
            @"UPDATE refresh_tokens
              SET revoked_at = CURRENT_TIMESTAMP, replaced_by_token_hash = @replacedByTokenHash
              WHERE token_hash = @tokenHash AND revoked_at IS NULL",
            new { tokenHash, replacedByTokenHash });

        return rows > 0;
    }

    public async Task<bool> RevokeAllForUserAsync(string userId)
    {
        if (!int.TryParse(userId, out var uid))
            return false;

        var rows = await ExecuteAsync(
            @"UPDATE refresh_tokens
              SET revoked_at = CURRENT_TIMESTAMP
              WHERE user_id = @uid AND revoked_at IS NULL",
            new { uid });

        return rows > 0;
    }
}
