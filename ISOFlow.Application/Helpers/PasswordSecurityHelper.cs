using System.Security.Cryptography;
using System.Text;

namespace ISOFlow.Application.Helpers;

public static class PasswordSecurityHelper
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a plain-text password using PBKDF2-HMAC-SHA256 with a unique cryptographic salt.
    /// Format: $pbkdf2-sha256$i=100000$[salt_base64]$[hash_base64]
    /// </summary>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            KeySize);

        return $"$pbkdf2-sha256$i={Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies an input password against a stored PBKDF2 hash. Anything not in the
    /// PBKDF2 format (e.g. a legacy plain-text value) never verifies.
    /// </summary>
    public static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(inputPassword) || !IsPasswordHash(storedHash))
            return false;

        try
        {
            var parts = storedHash.Split('$');
            // parts: ["", "pbkdf2-sha256", "i=100000", "[salt]", "[hash]"]
            if (parts.Length != 5)
                return false;

            var iterations = int.Parse(parts[2].Replace("i=", ""));
            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(inputPassword),
                salt,
                iterations,
                Algorithm,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>True when the stored value is a PBKDF2 hash produced by <see cref="HashPassword"/>.</summary>
    public static bool IsPasswordHash(string? storedValue) =>
        !string.IsNullOrEmpty(storedValue) && storedValue.StartsWith("$pbkdf2-sha256$", StringComparison.Ordinal);

    /// <summary>
    /// Generates a cryptographically random, unbiased reset token string.
    /// </summary>
    public static string GenerateResetToken(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return RandomNumberGenerator.GetString(chars, length);
    }

    /// <summary>
    /// SHA-256 hex digest of a reset token. Only this digest is persisted, never the token itself.
    /// </summary>
    public static string HashResetToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim().ToUpperInvariant())));
}

