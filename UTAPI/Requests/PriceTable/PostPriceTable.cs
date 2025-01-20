using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.PriceTable
{
    public class PostPriceTable
    {
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public int NLine { get; set; }

        [Required]
        public int NColumn { get; set; }

        [Required]
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}