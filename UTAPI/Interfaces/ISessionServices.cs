using UTAPI.Models;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing user sessions, including session creation, retrieval, and deactivation.
    /// </summary>
    public interface ISessionServices
    {
        /// <summary>
        /// Creates a new session for a user with the provided token and user ID.
        /// </summary>
        /// <param name="token">The token associated with the session.</param>
        /// <param name="userId">The ID of the user for whom the session is being created.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CreateSessionAsync(string token, Guid userId);

        /// <summary>
        /// Retrieves a list of sessions for a specific user, applying any provided filters.
        /// </summary>
        /// <param name="filter">The filters to apply for the query.</param>
        /// <param name="userId">The ID of the user whose sessions are being retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of sessions.</returns>
        Task<List<Session>> GetSessionsByUserAsync(FilterQuery? filter, Guid userId);

        /// <summary>
        /// Deactivates a session by its ID.
        /// </summary>
        /// <param name="id">The ID of the session to be deactivated.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeactivateSessionAsync(Guid id);
    }
}
