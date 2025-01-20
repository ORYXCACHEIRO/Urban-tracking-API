
namespace UTAPI.Requests.RouteLine
{
    public class PatchRouteLine
    {
        public double FirstLat { get; set; }       // Starting latitude
        public double FirstLong { get; set; }      // Starting longitude
        public int Direction { get; set; }         // Direction as an integer
        public string LineColor { get; set; }      // Color of the route line
        public double SecondLat { get; set; }      // Ending latitude
        public double SecondLong { get; set; }     // Ending longitude

        // Additional properties can be added as needed
    }
}
