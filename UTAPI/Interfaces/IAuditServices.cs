using UTAPI.Models;
using UTAPI.Types;

namespace UTAPI.Interfaces
{
    /// <summary>
    /// Defines methods for managing and retrieving audit records.
    /// </summary>
    public interface IAuditServices
    {
        /// <summary>
        /// Adds a collection of audit records to the system asynchronously.
        /// </summary>
        /// <param name="audits">The collection of <see cref="Audit"/> records to be added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddAuditsAsync(IEnumerable<Audit> audits);

        /// <summary>
        /// Retrieves a filtered list of audit records asynchronously.
        /// </summary>
        /// <param name="filter">The filtering criteria specified as a <see cref="FilterQuery"/> object. Can be null.</param>
        /// <returns>A task that returns a list of <see cref="Audit"/> records matching the filter criteria.</returns>
        Task<List<Audit>> GetAuditsAsync(FilterQuery? filter);
    }
}

