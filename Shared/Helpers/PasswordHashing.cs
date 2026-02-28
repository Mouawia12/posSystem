using System.Security.Cryptography;

namespace Shared.Helpers
{
    public static class PasswordHashing
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string encodedHash)
        {
            if (string.IsNullOrWhiteSpace(encodedHash))
            {
                return false;
            }

            var segments = encodedHash.Split('$');
            if (segments.Length != 4 || !segments[0].Equals("pbkdf2-sha256", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!int.TryParse(segments[1], out var iterations) || iterations <= 0)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(segments[2]);
                var expectedHash = Convert.FromBase64String(segments[3]);
                var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
                return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
