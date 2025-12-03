namespace NorthWind.Sales.Backend.Controllers.Membership;

public static class PasswordRules
{
    // Returns null when valid, otherwise error message.
    public static string? Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 4 || password.Length > 10)
            return "Password must be 4-10 chars with complexity.";
        bool upper = password.Any(char.IsUpper);
        bool lower = password.Any(char.IsLower);
        bool digit = password.Any(char.IsDigit);
        bool special = password.Any(c => !char.IsLetterOrDigit(c));
        if (!(upper && lower && digit && special))
            return "Password must include uppercase, lowercase, number and special char.";
        return null;
    }
}
