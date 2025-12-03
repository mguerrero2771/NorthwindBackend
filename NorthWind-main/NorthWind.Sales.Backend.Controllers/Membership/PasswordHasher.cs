using System.Security.Cryptography;
using System.Text;

namespace NorthWind.Sales.Backend.Controllers.Membership;

public static class PasswordHasher
{
    public static string GenerateSalt(int size = 16)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes);
    }

    public static string HashPassword(string password, string salt, int iterations = 100_000)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    public static bool Verify(string password, string salt, string hash)
    {
        var test = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash), Convert.FromBase64String(test));
    }
}
