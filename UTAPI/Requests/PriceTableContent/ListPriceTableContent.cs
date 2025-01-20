using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.PriceTableContent
{
    public class ListPriceTableContent
    {
        public Guid Id { get; set; }
        public Guid PriceTableId { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }
        public string Content { get; set; }
    }
}