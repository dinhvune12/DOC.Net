using System;
using System.Security.Cryptography;

namespace Duanlamchung.Utils
{
    public static class PasswordHasher
    {
        // Format: iterations.saltBase64.hashBase64
        public static string HashPassword(string password, int iterations = 10000)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                {
                    var hash = pbkdf2.GetBytes(32);
                    return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
                }
            }
        }

        public static bool VerifyPassword(string password, string hashed)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(hashed)) return false;
            var parts = hashed.Split('.');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out int iterations)) return false;
            var salt = Convert.FromBase64String(parts[1]);
            var storedHash = Convert.FromBase64String(parts[2]);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                var computed = pbkdf2.GetBytes(storedHash.Length);
                return AreEqual(storedHash, computed);
            }
        }

        private static bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}

