using UTAPI.Requests.Route;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing routes, including retrieval, creation, updating, and deletion.
    /// </summary>
    public interface IRouteServices
    {
        /// <summary>
        /// Retrieves a list of routes based on the provided filters.
        /// </summary>
        /// <param name="filter">Filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of routes.</returns>
        Task<List<ListRoute>> GetRoutesAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves a specific route by its ID.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>A task that represents the asynchronous operation, with the details of the route.</returns>
        Task<OneRoute> GetByIdAsync(Guid userId, string userRole, Guid routeId);

        /// <summary>
        /// Creates a new route.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="request">The data for the new route.</param>
        /// <returns>A task that represents the asynchronous operation, with the created route.</returns>
        Task<Models.Route> CreateRouteAsync(Guid userId, string userRole, PostRoute request);

        /// <summary>
        /// Updates an existing route.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="routeId">The ID of the route to be updated.</param>
        /// <param name="request">The data to update the route.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateRouteAsync(Guid userId, string userRole, Guid routeId, PatchRoute request);

        /// <summary>
        /// Deletes a route by its ID.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="routeId">The ID of the route to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteRouteAsync(Guid userId, string userRole, Guid routeId);

        /// <summary>
        /// Retrieves a specific route by its ID and logs the action in the user's route history.
        /// </summary>
        /// <param name="id">The ID of the route.</param>
        /// <param name="loggedUser">The ID of the authenticated user.</param>
        /// <returns>A task that represents the asynchronous operation, with the route details.</returns>
        Task<OneRoute> GetByIdMobileAsync(Guid id, Guid loggedUser);
    }
}
