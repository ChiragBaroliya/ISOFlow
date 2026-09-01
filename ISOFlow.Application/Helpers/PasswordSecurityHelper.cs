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
    /// Verifies an input password against stored hash or fallback plain-text for seed data.
    /// </summary>
    public static bool VerifyPassword(string inputPassword, string storedHashOrPassword)
    {
        if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHashOrPassword))
            return false;

        // Check if stored string is in PBKDF2 format
        if (storedHashOrPassword.StartsWith("$pbkdf2-sha256$"))
        {
            try
            {
                var parts = storedHashOrPassword.Split('$');
                // parts: ["", "pbkdf2-sha256", "i=100000", "[salt]", "[hash]"]
                if (parts.Length == 5)
                {
                    var iterationPart = parts[2];
                    var iterations = int.Parse(iterationPart.Replace("i=", ""));
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
            }
            catch
            {
                return false;
            }
        }

        // Backwards compatibility for plain-text seed passwords (e.g. "Test@123")
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(inputPassword),
            Encoding.UTF8.GetBytes(storedHashOrPassword));
    }

    /// <summary>
    /// Generates a cryptographically random reset token string.
    /// </summary>
    public static string GenerateResetToken(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            result.Append(chars[b % chars.Length]);
        }
        return result.ToString();
    }
}

