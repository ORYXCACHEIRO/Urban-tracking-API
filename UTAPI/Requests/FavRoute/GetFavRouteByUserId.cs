namespace UTAPI.Requests.FavRoute
{
    public class GetFavRouteByUserId
    {
        public Guid Id { get; set; }  // Identificador único da região
        public Guid UserId { get; set; }  // Identificador único da região
        public Guid RouteId { get; set; }  // Nome da região
    }
}
