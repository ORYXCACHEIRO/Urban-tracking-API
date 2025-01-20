using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents the content of a specific cell in a price table.
    /// </summary>
    public class PriceTableContent
    {
        /// <summary>
        /// Gets or sets the unique identifier for the PriceTableContent record.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated price table.
        /// </summary>
        [Required]
        public Guid PriceTableId { get; set; }

        /// <summary>
        /// Gets or sets the line number within the price table.
        /// </summary>
        [Required]
        public int Line { get; set; }

        /// <summary>
        /// Gets or sets the column number within the price table.
        /// </summary>
        [Required]
        public int Col { get; set; }

        /// <summary>
        /// Gets or sets the content of the cell in the price table.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// Navigation property to the associated PriceTable.
        /// </summary>
        public virtual PriceTable PriceTable { get; set; }
    }
}
