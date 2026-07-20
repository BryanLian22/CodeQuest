using System;
using System.Security.Cryptography;

namespace CodeQuest.Data
{
    /// <summary>
    /// PBKDF2 password hashing for dbo.User.password.
    /// Stored format: PBKDF2$iterations$saltBase64$hashBase64
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required.", "password");
            }

            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = deriveBytes.GetBytes(HashSize);
                return string.Format(
                    "PBKDF2${0}${1}${2}",
                    Iterations,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hash));
            }
        }

        public static bool Verify(string password, string storedValue)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedValue))
            {
                return false;
            }

            string[] parts = storedValue.Split('$');
            if (parts.Length != 4 || !parts[0].Equals("PBKDF2", StringComparison.Ordinal))
            {
                return false;
            }

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations < 10000)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] actualHash = deriveBytes.GetBytes(expectedHash.Length);
                return FixedTimeEquals(actualHash, expectedHash);
            }
        }

        private static bool FixedTimeEquals(byte[] first, byte[] second)
        {
            if (first == null || second == null || first.Length != second.Length)
            {
                return false;
            }

            int difference = 0;
            for (int index = 0; index < first.Length; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }
    }
}
