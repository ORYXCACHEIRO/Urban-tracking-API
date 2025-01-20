namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for hashing passwords and verifying hashed passwords.
    /// </summary>
    public interface IPasswordHelper
    {
        /// <summary>
        /// Hashes a plain text password.
        /// </summary>
        /// <param name="password">The plain text password to be hashed.</param>
        /// <returns>
        /// A hashed version of the input password.
        /// </returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies if the provided password matches the hashed password.
        /// </summary>
        /// <param name="hashedPassword">The hashed password to verify against.</param>
        /// <param name="password">The plain text password to verify.</param>
        /// <returns>
        /// <c>true</c> if the plain text password matches the hashed password; otherwise, <c>false</c>.
        /// </returns>
        bool VerifyPassword(string hashedPassword, string password);
    }
}
