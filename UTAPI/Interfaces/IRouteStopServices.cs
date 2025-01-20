using UTAPI.Models;
using UTAPI.Requests.RouteStop;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing route stops, including retrieval, creation, and deletion of stops on routes.
    /// </summary>
    public interface IRouteStopServices
    {
        /// <summary>
        /// Retrieves a list of route stops based on the provided filters.
        /// </summary>
        /// <param name="filter">Filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of route stops.</returns>
        Task<List<RouteStop>> GetRouteStopsAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves a specific route stop by its ID.
        /// </summary>
        /// <param name="id">The ID of the route stop.</param>
        /// <returns>A task that represents the asynchronous operation, with the details of the route stop.</returns>
        Task<RouteStop> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new stop to a route.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="request">The data for the new route stop.</param>
        /// <returns>A task that represents the asynchronous operation, with the created route stop.</returns>
        Task<RouteStop> CreateRouteStopAsync(Guid userId, string userRole, PostRouteStop request);

        /// <summary>
        /// Removes a stop from a route.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="userRole">The role of the authenticated user.</param>
        /// <param name="routeStopId">The ID of the route stop to be removed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteRouteStopAsync(Guid userId, string userRole, Guid routeStopId);

        /// <summary>
        /// Retrieves a list of route stops associated with a specific route ID.
        /// </summary>
        /// <param name="routeId">The ID of the route.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of route stops for the specified route.</returns>
        Task<List<ListRouteStop>> GetRouteStopsByRouteIdAsync(Guid routeId);
    }
}
