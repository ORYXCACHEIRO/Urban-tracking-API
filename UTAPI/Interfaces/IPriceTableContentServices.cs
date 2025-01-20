using UTAPI.Requests.PriceTable;
using UTAPI.Requests.PriceTableContent;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing price table content, including creation, retrieval, updating, and deletion.
    /// </summary>
    public interface IPriceTableContentServices
    {
        /// <summary>
        /// Creates new price table content.
        /// </summary>
        /// <param name="request">The data required to create the price table content.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation, with the created price table content.</returns>
        Task<ListPriceTableContent> CreatePriceTableContentAsync(PostPriceTableContent request, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves the content of a price table based on its ID.
        /// </summary>
        /// <param name="priceTableId">The ID of the price table.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of price table content.</returns>
        Task<List<ListPriceTableContent>> GetPriceTableContentByPriceTableIdAsync(Guid priceTableId, Guid loggedUserId, string userRole);

        /// <summary>
        /// Retrieves all price table contents, possibly filtered by the provided query.
        /// </summary>
        /// <param name="filter">The filter query to apply (optional).</param>
        /// <returns>A task that represents the asynchronous operation, with a list of all price table contents.</returns>
        Task<List<ListPriceTableContent>> GetPriceTablesContentAsync(FilterQuery? filter);

        /// <summary>
        /// Deletes the content of a price table by its ID.
        /// </summary>
        /// <param name="id">The ID of the price table content to be deleted.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeletePriceTableContentAsync(Guid id, Guid loggedUserId, string userRole);

        /// <summary>
        /// Updates the content of a price table.
        /// </summary>
        /// <param name="id">The ID of the price table content to be updated.</param>
        /// <param name="request">The data to update the price table content.</param>
        /// <param name="loggedUserId">The ID of the logged-in user.</param>
        /// <param name="userRole">The role of the logged-in user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdatePriceTableContentAsync(Guid id, PatchPriceTableContent request, Guid loggedUserId, string userRole);
    }
}
