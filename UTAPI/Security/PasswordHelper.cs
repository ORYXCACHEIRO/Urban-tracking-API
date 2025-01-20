using BCrypt.Net;
using UTAPI.Interfaces;

namespace UTAPI.Security
{
    public class PasswordHelper : IPasswordHelper
    {
        // Method to hash a password
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Method to verify a password against a stored hash
        public bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }

}
