using UTAPI.Requests.EntityDriver;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Defines methods for managing entity drivers, including creation, retrieval, and deletion.
    /// </summary>
    public interface IEntityDriverServices
    {
        /// <summary>
        /// Creates a new entity driver association.
        /// </summary>
        /// <param name="request">The request containing details of the entity driver to create.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a <see cref="ListEntityDriver"/> object representing the created entity driver.
        /// </returns>
        Task<ListEntityDriver> CreateEntityDriverAsync(PostEntityDriver request, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves entity drivers associated with a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose entity drivers are being retrieved.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListEntityDriver"/> objects associated with the specified user.
        /// </returns>
        Task<List<ListEntityDriver>> GetEntityDriverByUserIdAsync(Guid userId, Guid loggedUserId, string userRole);

        /// <summary>
        /// Deletes an entity driver association by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity driver association to delete.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteEntityDriverAsync(Guid id, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves all entity drivers associated with a specific entity.
        /// </summary>
        /// <param name="entityId">The unique identifier of the entity whose drivers are being retrieved.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListEntityDriver"/> objects associated with the specified entity.
        /// </returns>
        Task<List<ListEntityDriver>> GetEntityDriversByEntityIdAsync(Guid entityId, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves all entity drivers.
        /// </summary>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="filter">Filters to help find results in the table</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListEntityDriver"/> objects.
        /// </returns>
        Task<List<ListEntityDriver>> GetEntityDriversAsync(Guid loggedUserId, string userRole, FilterQuery? filter);
    }
}