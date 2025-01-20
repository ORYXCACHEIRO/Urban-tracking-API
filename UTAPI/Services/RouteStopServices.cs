using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.RouteStop;
using UTAPI.Types;
using AutoMapper.QueryableExtensions;

namespace UTAPI.Services
{
    public class RouteStopServices : IRouteStopServices
    {
        private readonly DataContext _context;
        private readonly ILogger<RouteStopServices> _logger;
        private readonly IMapper _mapper;

        public RouteStopServices(DataContext context, ILogger<RouteStopServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Implementing the CreateRouteStopAsync for adding a stop to a route
        public async Task<RouteStop> CreateRouteStopAsync(Guid userId, string userRole, PostRouteStop request)
        {
            try
            {
                // Validate route and stop existence
                var route = await _context.Route.FindAsync(request.RouteId);
                var stop = await _context.Stop.FindAsync(request.StopId);

                if (route == null) throw new Exception("Route not found");
                if (stop == null) throw new Exception("Stop not found");

                // Permission check for Entity Admin
                if (userRole == "2") // Entity Admin
                {
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == route.EntityId);
                    if (!isEntityDriver)
                        throw new UnauthorizedAccessException("User is not authorized to add stops to this route.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new UnauthorizedAccessException("User is not authorized to add stops to routes.");
                }

                // Check for duplicate RouteStop
                var routeStopExists = await _context.RouteStop
                    .AnyAsync(rs => rs.RouteId == request.RouteId && rs.StopId == request.StopId);

                if (routeStopExists)
                {
                    throw new Exception("This stop is already associated with the route.");
                }

                // Create the new RouteStop association
                var routeStop = new RouteStop
                {
                    RouteId = request.RouteId,
                    StopId = request.StopId
                };

                _context.RouteStop.Add(routeStop);
                await _context.SaveChangesAsync();
                return routeStop;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to create a RouteStop");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the RouteStop association");
                throw new Exception("An error occurred while creating the RouteStop association", ex);
            }
        }



        // Implementing the DeleteRouteStopAsync for removing a stop from a route
        public async Task DeleteRouteStopAsync(Guid userId, string userRole, Guid routeStopId)
        {
            try
            {
                // Fetch the RouteStop and associated Route
                var routeStop = await _context.RouteStop
                    .FirstOrDefaultAsync(rs => rs.Id == routeStopId);

                if (routeStop == null) throw new Exception("RouteStop association not found");

                var route = await _context.Route
                    .FirstOrDefaultAsync(r => r.Id == routeStop.RouteId);

                if (route == null) throw new Exception("Associated Route not found");

                // Permission check for Entity Admin
                if (userRole == "2") // Entity Admin
                {
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == route.EntityId);
                    if (!isEntityDriver)
                        throw new UnauthorizedAccessException("User is not authorized to delete this RouteStop association.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new UnauthorizedAccessException("User is not authorized to delete RouteStop associations.");
                }

                // Remove the RouteStop association
                _context.RouteStop.Remove(routeStop);
                await _context.SaveChangesAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to delete a RouteStop");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the RouteStop association");
                throw new Exception("An error occurred while deleting the RouteStop association", ex);
            }
        }

        public async Task<List<RouteStop>> GetRouteStopsAsync(FilterQuery? filter)
        {
            try
            {
                var routeStopsQuery = _context.RouteStop.AsQueryable();

                if (filter != null)
                {
                    if (filter.Limit.HasValue && filter.Limit.Value > 0)
                    {
                        int skipCount = (filter.NPage.GetValueOrDefault(1) - 1) * filter.Limit.Value;
                        routeStopsQuery = routeStopsQuery.Skip(skipCount).Take(filter.Limit.Value);
                    }
                }

                var routeStops = await routeStopsQuery.ToListAsync();
                if (routeStops == null || routeStops.Count == 0) return new List<RouteStop>();
                return routeStops;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving route-stop associations");
                throw new Exception("An error occurred while retrieving route-stop associations");
            }
        }

        public async Task<RouteStop> GetByIdAsync(Guid id)
        {
            try
            {
                var routeStop = await _context.RouteStop.FindAsync(id);

                if (routeStop == null) throw new Exception("RouteStop not found");

                return routeStop;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the route-stop association");
                throw new Exception("An error occurred while retrieving the route-stop association");
            }
        }

        public async Task<List<ListRouteStop>> GetRouteStopsByRouteIdAsync(Guid routeId)
        {
            try
            {
                var routeStops = await _context.RouteStop
                    .Where(rs => rs.RouteId == routeId)
                    .Include(rs => rs.Stop)
                    .ProjectTo<ListRouteStop>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                if (routeStops == null || routeStops.Count == 0) return new List<ListRouteStop>();

                return routeStops;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving route-stop associations by RouteId");
                throw new Exception("An error occurred while retrieving route-stop associations by RouteId");
            }
        }

    }
}
