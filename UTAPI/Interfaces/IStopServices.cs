using UTAPI.Models;
using UTAPI.Requests.Stop;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing stops, including creating, updating, retrieving, and deleting stops.
    /// </summary>
    public interface IStopServices
    {
        /// <summary>
        /// Retrieves a list of stops based on the provided filters.
        /// </summary>
        /// <param name="filter">The filters to apply for the query.</param>
        /// <returns>A task that represents the asynchronous operation, with a list of stops.</returns>
        Task<List<ListStop>> GetStopsAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves a specific stop by its ID.
        /// </summary>
        /// <param name="id">The ID of the stop to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, with details of the stop.</returns>
        Task<OneStop> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new stop with the provided data.
        /// </summary>
        /// <param name="request">The data for the new stop.</param>
        /// <returns>A task that represents the asynchronous operation, with the created stop.</returns>
        Task<Stop> CreateStopAsync(PostStop request);

        /// <summary>
        /// Updates an existing stop with the specified data.
        /// </summary>
        /// <param name="id">The ID of the stop to update.</param>
        /// <param name="request">The updated data for the stop.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateStopAsync(Guid id, PatchStop request);

        /// <summary>
        /// Deletes a stop by its ID.
        /// </summary>
        /// <param name="id">The ID of the stop to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteStopAsync(Guid id);
    }
}
