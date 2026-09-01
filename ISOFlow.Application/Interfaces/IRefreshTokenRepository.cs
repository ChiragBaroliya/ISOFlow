using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<bool> UpdateAsync(RefreshToken refreshToken);
    Task<bool> RevokeByTokenHashAsync(string tokenHash, string? replacedByTokenHash = null);
    Task<bool> RevokeAllForUserAsync(string userId);
}
