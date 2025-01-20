using UTAPI.Models;
using UTAPI.Requests.RouteLine;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing route lines, including retrieval, creation, and deletion.
    /// </summary>
    public interface IRouteLineServices
    {
        /// <summary>
        /// Retrieves a list of route lines based on the provided filters for the entity associated with the admin.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="filter">Filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of route lines associated with the entity of the admin.</returns>
        Task<List<ListRouteLine>> GetRouteLinesForEntityAdminAsync(Guid userId, FilterQuery? filter);

        /// <summary>
        /// Retrieves a list of route lines based on the provided filters. (Admin only)
        /// </summary>
        /// <param name="filter">Filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of route lines.</returns>
        Task<List<ListRouteLine>> GetRouteLinesAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves a list of route lines based on the provided filters and a specific route ID.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <param name="filter">Filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of route lines.</returns>
        Task<List<ListRouteLine>> GetByRouteIdAsync(Guid routeId, FilterQuery? filter);

        /// <summary>
        /// Retrieves a specific route line by its ID.
        /// </summary>
        /// <param name="id">The ID of the route line.</param>
        /// <returns>A task that represents the asynchronous operation, with the details of the route line.</returns>
        Task<OneRouteLine> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new route line.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="request">The data for the new route line.</param>
        /// <returns>A task that represents the asynchronous operation, with the created route line.</returns>
        Task<RouteLine> CreateRouteLineAsync(Guid userId, string userRole, PostRouteLine request);

        /// <summary>
        /// Deletes a route line by its ID.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="id">The ID of the route line to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteRouteLineAsync(Guid userId, string userRole, Guid id);
    }
}
