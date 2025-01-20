using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.Stop
{
    public class PatchStop
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }
    }
}
