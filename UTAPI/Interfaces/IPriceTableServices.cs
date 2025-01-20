using UTAPI.Requests.PriceTable;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing price tables, including creation, retrieval, updating, and deletion.
    /// </summary>
    public interface IPriceTableServices
    {
        /// <summary>
        /// Creates a new price table.
        /// </summary>
        /// <param name="request">The data required to create the price table.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation, with the created price table.</returns>
        Task<ListPriceTable> CreatePriceTableAsync(PostPriceTable request, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves a list of price tables, possibly filtered by the provided query.
        /// </summary>
        /// <param name="filter">The filter query to apply (optional).</param>
        /// <returns>A task that represents the asynchronous operation, with a list of price tables.</returns>
        Task<List<ListPriceTable>> GetPriceTablesAsync(FilterQuery? filter);

        /// <summary>
        /// Deletes a price table by its ID.
        /// </summary>
        /// <param name="id">The ID of the price table to be deleted.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeletePriceTableAsync(Guid id, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves a specific price table by its ID.
        /// </summary>
        /// <param name="id">The ID of the price table.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation, with the requested price table.</returns>
        Task<ListPriceTable> GetPriceTableByIdAsync(Guid id, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves price tables associated with a specific entity, possibly filtered by the provided query.
        /// </summary>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="filter">The filter query to apply (optional).</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of price tables associated with the entity.</returns>
        Task<List<ListPriceTable>> GetPriceTableByEntityIdAsync(Guid entityId, FilterQuery? filter, string userRole);

        /// <summary>
        /// Updates an existing price table.
        /// </summary>
        /// <param name="id">The ID of the price table to be updated.</param>
        /// <param name="request">The data to update the price table.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdatePriceTableAsync(Guid id, PatchPriceTable request, Guid loggedUserId, string userRole);
    }
}
