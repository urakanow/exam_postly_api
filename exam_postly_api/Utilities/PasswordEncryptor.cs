using System.Security.Cryptography;

namespace exam_postly_api.Utilities
{
    public class PasswordEncryptor
    {
        public static (string hashedPassword, string salt) EncryptPassword(string password)
        {
            // Generate a 128-bit salt using a cryptographically strong random sequence
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            var rfc2898 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] hashedBytes = rfc2898.GetBytes(32);

            string hashedPassword = Convert.ToBase64String(hashedBytes);

            return (hashedPassword, salt);
        }

        public static bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            try
            {
                // Convert stored salt and hash from Base64
                byte[] saltBytes = Convert.FromBase64String(salt);
                byte[] storedHashBytes = Convert.FromBase64String(hashedPassword);

                // Hash the entered password with the stored salt
                var rfc2898 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
                byte[] enteredHashBytes = rfc2898.GetBytes(32);

                // Compare the hashes in constant time to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(enteredHashBytes, storedHashBytes);
            }
            catch
            {
                // Handle any conversion errors (malformed salt/hash)
                return false;
            }
        }
    }
}