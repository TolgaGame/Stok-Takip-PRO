using System.Security.Cryptography;
using System.Text;

namespace StokTakipPro.Services;

public interface IAuthService
{
    bool Authenticate(string? username, string? password);
}

public class AuthService : IAuthService
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "admin123";

    public bool Authenticate(string? username, string? password)
    {
        var usernameMatches = AreEqual(username?.Trim() ?? string.Empty, ValidUsername);
        var passwordMatches = AreEqual(password ?? string.Empty, ValidPassword);
        return usernameMatches & passwordMatches;
    }

    private static bool AreEqual(string actual, string expected) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected));
}
