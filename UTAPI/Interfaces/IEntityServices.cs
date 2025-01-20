using UTAPI.Models;
using UTAPI.Requests.Entity;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Defines methods for managing entities, including creation, retrieval, updating, and deletion.
    /// </summary>
    public interface IEntityServices
    {
        /// <summary>
        /// Retrieves a list of entities, optionally filtered by the provided criteria.
        /// </summary>
        /// <param name="filter">The filter criteria to apply when retrieving entities.</param>
        /// <returns>
        /// A task that returns a list of <see cref="ListEntity"/> objects representing the retrieved entities.
        /// </returns>
        Task<List<ListEntity>> GetEntitiesAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves the entity associated with a specific user ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A task that returns a <see cref="OneEntity"/> object representing the retrieved entity.
        /// </returns>
        Task<OneEntity> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>
        /// A task that returns a <see cref="OneEntity"/> object representing the retrieved entity.
        /// </returns>
        Task<OneEntity> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        /// <param name="request">The request containing details of the entity to create.</param>
        /// <returns>
        /// A task that returns the newly created <see cref="Entity"/> object.
        /// </returns>
        Task<Entity> CreateEntityAsync(PostEntity request);

        /// <summary>
        /// Updates an existing entity with the provided data.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to update.</param>
        /// <param name="request">The data to update the entity with.</param>
        /// <param name="loggedUserId">The unique identifier of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>
        /// A task that represents the asynchronous update operation.
        /// </returns>
        Task UpdateEntityAsync(Guid id, PatchEntity request, Guid loggedUserId, string userRole);

        /// <summary>
        /// Deletes an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>
        /// A task that represents the asynchronous delete operation.
        /// </returns>
        Task DeleteEntityAsync(Guid id);
    }
}
