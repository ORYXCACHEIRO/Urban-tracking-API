using UTAPI.Requests.Region;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Provides methods for managing regions, including creation, retrieval, updating, and deletion.
    /// </summary>
    public interface IRegionServices
    {
        /// <summary>
        /// Retrieves a list of regions, possibly filtered by the provided query.
        /// </summary>
        /// <param name="filter">The filter query to apply (optional).</param>
        /// <returns>A task that represents the asynchronous operation, with a list of regions.</returns>
        Task<List<ListRegion>> GetRegionsAsync(FilterQuery? filter);

        /// <summary>
        /// Retrieves a specific region by its ID.
        /// </summary>
        /// <param name="id">The ID of the region to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, with the requested region.</returns>
        Task<OneRegion> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new region.
        /// </summary>
        /// <param name="request">The data required to create the region.</param>
        /// <returns>A task that represents the asynchronous operation, with the created region.</returns>
        Task<OneRegion> CreateRegionAsync(PostRegion request);

        /// <summary>
        /// Updates an existing region.
        /// </summary>
        /// <param name="id">The ID of the region to update.</param>
        /// <param name="request">The data to update the region.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateRegionAsync(Guid id, PatchRegion request);

        /// <summary>
        /// Deletes a region by its ID.
        /// </summary>
        /// <param name="id">The ID of the region to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteRegionAsync(Guid id);
    }
}
