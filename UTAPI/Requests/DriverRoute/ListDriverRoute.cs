namespace UTAPI.Requests.DriverRoute
{
    public class ListDriverRoute
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RouteId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string WorkDays { get; set; }
        public string Username { get; set; } // nome do user
    }
}
