using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.PriceTable
{
    public class ListPriceTable
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public int NLine { get; set; }
        public int NColumn { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}