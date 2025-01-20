using UTAPI.Models;
using UTAPI.Requests.RouteHistory;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing route history, including creation, retrieval, and deletion.
    /// </summary>
    public interface IRouteHistoryServices
    {
        /// <summary>
        /// Creates a new route history entry.
        /// </summary>
        /// <param name="request">The data required to create the route history entry.</param>
        /// <returns>A task that represents the asynchronous operation, with the created route history entry.</returns>
        Task<RouteHistory> CreateRouteHistoryAsync(PostRouteHistory request);

        /// <summary>
        /// Retrieves the route history for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose route history is to be retrieved.</param>
        /// <param name="loggedUserId">The ID of the logged-in user requesting the route history.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of route history entries.</returns>
        Task<List<RouteHistory>> GetRouteHistoryByUserIdAsync(Guid userId, Guid loggedUserId);

        /// <summary>
        /// Deletes a specific route history entry for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose route history entry is to be deleted.</param>
        /// <param name="routeId">The ID of the route whose history entry is to be deleted.</param>
        /// <param name="loggedUserId">The ID of the logged-in user requesting the deletion.</param>
        /// <returns>A task that represents the asynchronous operation, with a boolean indicating success or failure.</returns>
        Task<bool> DeleteRouteHistoryAsync(Guid userId, Guid routeId, Guid loggedUserId);
    }
}
