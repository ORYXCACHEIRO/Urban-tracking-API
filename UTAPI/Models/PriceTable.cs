using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a price table associated with an entity.
    /// </summary>
    public class PriceTable
    {
        /// <summary>
        /// Gets or sets the unique identifier for the PriceTable record.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated entity.
        /// </summary>
        [Required]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the number of lines in the price table.
        /// </summary>
        [Required]
        public int NLine { get; set; }

        /// <summary>
        /// Gets or sets the number of columns in the price table.
        /// </summary>
        [Required]
        public int NColumn { get; set; }

        /// <summary>
        /// Gets or sets the name of the price table.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the price table is active.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        /// <summary>
        /// Navigation property to the associated Entity.
        /// </summary>
        public virtual Entity Entity { get; set; }
    }
}
