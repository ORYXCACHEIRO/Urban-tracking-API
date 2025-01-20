namespace UTAPI.Requests.RouteStop
{
    public class ListRouteStop
    {
        public Guid Id { get; set; }

        public Guid RouteId { get; set; }

        public Guid StopId { get; set; }

        public bool Active { get; set; }  // Optional for filtering, no need to be required
        public string? RouteName { get; set; }
        public string? StopName { get; set; }
    }
}
