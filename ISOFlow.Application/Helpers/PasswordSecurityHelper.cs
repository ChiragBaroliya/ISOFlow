using System.Security.Cryptography;
using System.Text;

namespace ISOFlow.Application.Helpers;

public static class PasswordSecurityHelper
{
    public static bool VerifyPassword(string inputPassword, string storedHashOrPassword)
    {
        if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHashOrPassword))
            return false;
        return string.Equals(inputPassword, storedHashOrPassword, StringComparison.Ordinal);
    }

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
