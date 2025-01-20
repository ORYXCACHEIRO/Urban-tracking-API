using UTAPI.Requests.User;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing users, including creating, updating, retrieving, and deleting users and drivers.
    /// </summary>
    public interface IUserServices
    {
        /// <summary>
        /// Retrieves a list of users based on the provided filters.
        /// </summary>
        /// <param name="filter">The filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of users.</returns>
        Task<List<ListUser>> GetUsersAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves a list of drivers based on the provided filters.
        /// This method is used by the entity admin only.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user (admin).</param>
        /// <param name="filter">The filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of drivers.</returns>
        Task<List<ListUser>> GetDriversAsync(Guid loggedInUserId, FilterQuery? filter);

        /// <summary>
        /// Retrieves a specific user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="role">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation, with the user details.</returns>
        Task<OneUser> GetByIdAsync(Guid id, Guid loggedInUserId, string role);

        /// <summary>
        /// Retrieves a specific driver by their ID.
        /// This method is used by the entity admin only.
        /// </summary>
        /// <param name="id">The ID of the driver to retrieve.</param>
        /// <param name="loggedInUserId">The ID of the logged-in user (admin).</param>
        /// <returns>A task that represents the asynchronous operation, with the driver details.</returns>
        Task<OneUser> GetDriverByIdAsync(Guid id, Guid loggedInUserId);

        /// <summary>
        /// Retrieves a list of unassociated users based on the provided filters.
        /// </summary>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <param name="filter">The filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of unassociated users.</returns>
        Task<List<ListUser>> GetUnassociatedUsersAsync(Guid loggedUserId, string userRole, FilterQuery? filter);

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="request">The data for the new user.</param>
        /// <returns>A task that represents the asynchronous operation, with the created user.</returns>
        Task<OneUser> CreateUserAsync(PostUser request);

        /// <summary>
        /// Creates a new driver.
        /// This method is used by the entity admin only.
        /// </summary>
        /// <param name="request">The data for the new driver.</param>
        /// <param name="loggedInUserId">The ID of the logged-in user (admin).</param>
        /// <returns>A task that represents the asynchronous operation, with the created driver.</returns>
        Task<OneUser> CreateDriverAsync(PostUser request, Guid loggedInUserId);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="logUser">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <param name="request">The updated data for the user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateUserAsync(Guid id, Guid logUser, string userRole, PatchUser request);

        /// <summary>
        /// Updates an existing driver.
        /// This method is used by the entity admin only.
        /// </summary>
        /// <param name="id">The ID of the driver to update.</param>
        /// <param name="loggedInAdminId">The ID of the logged-in admin.</param>
        /// <param name="request">The updated data for the driver.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateDriverAsync(Guid id, Guid loggedInAdminId, PatchUser request);

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteUserAsync(Guid id, Guid loggedInUserId);

        /// <summary>
        /// Deletes a driver by their ID.
        /// This method is used by the entity admin only.
        /// </summary>
        /// <param name="id">The ID of the driver to delete.</param>
        /// <param name="loggedInUserId">The ID of the logged-in user (admin).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteDriverAsync(Guid id, Guid loggedInUserId);
    }
}
