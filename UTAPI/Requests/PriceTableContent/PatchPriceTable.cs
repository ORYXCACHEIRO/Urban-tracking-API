using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.PriceTable
{
    public class PatchPriceTableContent
    {
        public Guid PriceTableId { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }
        public string Content { get; set; }
    }
}