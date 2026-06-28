using System.Security.Cryptography;

namespace EduCore.Helpers
{
    /// <summary>
    /// PBKDF2 (SHA-256) password hashing. Stores "base64(salt):base64(hash)".
    /// No external packages required.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;       // 128-bit salt
        private const int KeySize = 32;        // 256-bit hash
        private const int Iterations = 100_000;
        private const char Delimiter = ':';
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            var parts = storedHash.Split(Delimiter);
            if (parts.Length != 2)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[0]);
                var key = Convert.FromBase64String(parts[1]);
                var input = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
                return CryptographicOperations.FixedTimeEquals(key, input);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
