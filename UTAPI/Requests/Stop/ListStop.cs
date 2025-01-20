using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.Stop
{
    public class ListStop
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
