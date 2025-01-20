using UTAPI.Models;
using UTAPI.Requests.Auth;
using UTAPI.Requests.User;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Defines methods for authentication, user management, and user-related operations.
    /// </summary>
    public interface IAuthServices
    {
        /// <summary>
        /// Checks the credentials of a user during the login process.
        /// </summary>
        /// <param name="request">The login request containing the user's credentials.</param>
        /// <returns>
        /// A task that returns a <see cref="User"/> object if the credentials are valid.
        /// </returns>
        Task<User> CheckUserAsync(Login request);

        /// <summary>
        /// Retrieves information about the currently logged-in user.
        /// </summary>
        /// <param name="userId">The unique identifier of the logged-in user.</param>
        /// <returns>
        /// A task that returns a <see cref="OneUser"/> object containing detailed information about the user.
        /// </returns>
        Task<OneUser> GetMeInfoAsync(Guid userId);

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The registration request containing the user's details.</param>
        /// <returns>A task that represents the asynchronous registration process.</returns>
        Task RegisterAsync(Register request);
    }
}
