using Microsoft.AspNetCore.Mvc;

namespace UTAPI.Types
{
    /// <summary>
    /// Represents the query parameters used for filtering, pagination, and sorting in API requests.
    /// </summary>
    public class FilterQuery
    {
        /// <summary>
        /// Gets or sets the page number for pagination.
        /// </summary>
        [FromQuery(Name = "page")]
        public int? NPage { get; set; }

        /// <summary>
        /// Gets or sets the limit of items per page for pagination.
        /// </summary>
        [FromQuery(Name = "limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets the sorting criteria for the query. 
        /// Can specify fields to sort by, and the sorting direction (e.g., "name asc" or "date desc").
        /// </summary>
        [FromQuery(Name = "sort")]
        public string? Sort { get; set; }

        /// <summary>
        /// Gets or sets the filtering condition in the query, used to filter the data.
        /// </summary>
        [FromQuery(Name = "where")]
        public string? Where { get; set; }
    }
}
