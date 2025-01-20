using UTAPI.Requests.DriverRoute;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Defines methods for managing driver routes, including creation, retrieval, and deletion.
    /// </summary>
    public interface IDriverRouteServices
    {
        /// <summary>
        /// Creates a new driver route.
        /// </summary>
        /// <param name="request">The request containing details of the driver route to create.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a <see cref="ListDriverRoute"/> object representing the created driver route.
        /// </returns>
        Task<ListDriverRoute> CreateDriverRouteAsync(PostDriverRoute request, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves a list of driver routes associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose routes are being retrieved.</param>
        /// <param name="filter">Optional filters to apply to the retrieved routes.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListDriverRoute"/> objects.
        /// </returns>
        Task<List<ListDriverRoute>> GetDriverRoutesByUserAsync(Guid userId, FilterQuery? filter, string userRole, Guid loggedUserId);

        /// <summary>
        /// Retrieves details of a specific driver route by its unique identifier.
        /// </summary>
        /// <param name="driverRouteId">The unique identifier of the driver route.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a <see cref="ListDriverRoute"/> object containing details of the specified route.
        /// </returns>
        Task<ListDriverRoute> GetDriverRouteByIdAsync(Guid driverRouteId, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves details of all driver routes.
        /// </summary>
        /// <param name="filter">Optional filters to apply to the retrieved routes.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListDriverRoute"/> objects.
        /// </returns>
        Task<List<ListDriverRoute>> GetDriverRoutesAsync(string userRole, FilterQuery? filter);

        /// <summary>
        /// Retrieves details of all driver routes based on an routeId.
        /// </summary>
        /// <param name="routeId">Route id for filtering.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="filter">Optional filters to apply to the retrieved routes.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListDriverRoute"/> objects .
        /// </returns>
        Task<List<ListDriverRoute>> GetDriverRoutesByRouteIdAsync(Guid routeId, Guid loggedUserId, string userRole, FilterQuery? filter);

        /// <summary>
        /// Deletes a driver route by its unique identifier.
        /// </summary>
        /// <param name="driverRouteId">The unique identifier of the driver route to delete.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteDriverRouteAsync(Guid driverRouteId, Guid loggedUserId, string userRole);
    }
}
