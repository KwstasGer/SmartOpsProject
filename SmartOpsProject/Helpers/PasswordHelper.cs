using Microsoft.AspNetCore.Identity;

namespace SmartOps.Helpers
{
    public static class PasswordHelper
    {
        // Προτιμώ αντικείμενο αντί για string, δεν αλλάζει η ουσία.
        private static readonly PasswordHasher<object> _hasher = new();

        public static string HashPassword(string password)
            => _hasher.HashPassword(null!, password);

        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success
                || result == PasswordVerificationResult.SuccessRehashNeeded;
        }

        // Optional: helper για rehash όταν χρειάζεται
        public static (bool ok, string? newHash) VerifyAndMaybeRehash(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
            if (result == PasswordVerificationResult.Success)
                return (true, null);
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
                return (true, _hasher.HashPassword(null!, providedPassword));
            return (false, null);
        }
    }
}
