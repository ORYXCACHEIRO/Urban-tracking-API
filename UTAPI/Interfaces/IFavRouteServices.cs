using UTAPI.Models;
using UTAPI.Requests.FavRoute;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing favorite routes, including creation, retrieval, and deletion.
    /// </summary>
    public interface IFavRouteServices
    {
        /// <summary>
        /// Creates a new favorite route for a user.
        /// </summary>
        /// <param name="request">The data of the new favorite route.</param>
        /// <param name="loggedUserId">The ID of the currently authenticated user.</param>
        /// <returns>
        /// A task that represents the asynchronous creation of the favorite route.
        /// The result is a <see cref="FavRoute"/> object representing the newly created favorite route.
        /// </returns>
        Task<FavRoute> CreateFavRouteAsync(PostFavRoute request, Guid loggedUserId);

        /// <summary>
        /// Retrieves a specific favorite route for a user based on their user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="loggedUserId">The ID of the currently authenticated user.</param>
        /// <returns>
        /// A task that returns a list of <see cref="GetFavRouteByUserId"/> objects representing the favorite routes for the user.
        /// </returns>
        Task<List<GetFavRouteByUserId>> GetFavRouteByUserIdAsync(Guid userId, Guid loggedUserId);

        /// <summary>
        /// Deletes a favorite route by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the favorite route to be deleted.</param>
        /// <param name="loggedUserId">The ID of the currently authenticated user.</param>
        /// <returns>
        /// A task that represents the asynchronous deletion of the favorite route.
        /// </returns>
        Task DeleteFavRouteAsync(Guid id, Guid loggedUserId);
    }
}
