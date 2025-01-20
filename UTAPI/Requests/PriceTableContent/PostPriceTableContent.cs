using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.PriceTable
{
    public class PostPriceTableContent
    {
        [Required]
        public Guid PriceTableId { get; set; }

        [Required]
        public int Line { get; set; }

        [Required]
        public int Col { get; set; }

        [Required]
        public string Content { get; set; }
    }
}