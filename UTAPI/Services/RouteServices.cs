using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Interfaces;
using UTAPI.Requests.Route;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class RouteServices : IRouteServices
    {
        private readonly DataContext _context;
        private readonly ILogger<RouteServices> _logger;
        private readonly IMapper _mapper;
        private readonly IRouteHistoryServices _routeHistoryServices;

        public RouteServices(
            DataContext context,
            ILogger<RouteServices> logger,
            IMapper mapper,
            IRouteHistoryServices routeHistoryServices,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _routeHistoryServices = routeHistoryServices;
        }

        public async Task<Models.Route> CreateRouteAsync(Guid userId, string userRole, PostRoute request)
        {
            try
            {
                // Validate permissions
                if (userRole == "2") // Entity Admin
                {
                    var isEntityDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == userId && ed.EntityId == request.EntityId);
                    if (isEntityDriver == null) throw new Utils.UnauthorizedAccessException("User is not authorized for this entity.");
                    request.EntityId = isEntityDriver.EntityId;
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new Utils.UnauthorizedAccessException("User is not authorized to create routes.");
                }

                // Validate Region and Entity existence
                var regionExists = await _context.Region.AnyAsync(r => r.Id == request.RegionId);
                if (!regionExists) throw new Exception("Region not found");

                var entityExists = await _context.Entity.AnyAsync(e => e.Id == request.EntityId);
                if (!entityExists) throw new Exception("Entity not found");

                // Map and create route
                var route = _mapper.Map<Models.Route>(request);
                _context.Route.Add(route);
                await _context.SaveChangesAsync();

                return route;
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the route");
                throw new Exception("An error occurred while creating the route", ex);
            }
        }


        public async Task DeleteRouteAsync(Guid userId, string userRole, Guid routeId)
        {
            try
            {
                // Fetch the route
                var route = await _context.Route.FindAsync(routeId);
                if (route == null) throw new Exception("Route not found");

                // Validate permissions
                if (userRole == "2") // Entity Admin
                {
                    // Check if the user is linked to the route's entity as an EntityDriver
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == route.EntityId);
                    if (!isEntityDriver) throw new Utils.UnauthorizedAccessException("User is not authorized to delete this route.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new Utils.UnauthorizedAccessException("User is not authorized to delete routes.");
                }

                // Check for dependent records
                var hasRouteStops = await _context.RouteStop.AnyAsync(rs => rs.RouteId == routeId);
                var hasRouteLines = await _context.RouteLine.AnyAsync(rl => rl.RouteId == routeId);
                var hasDriverRoutes = await _context.DriverRoute.AnyAsync(dr => dr.RouteId == routeId);

                if (hasRouteStops || hasRouteLines || hasDriverRoutes)
                {
                    throw new InvalidOperationException("Cannot delete route with existing dependencies (stops, lines, or driver assignments).");
                }

                // Perform deletion
                _context.Route.Remove(route);
                await _context.SaveChangesAsync();
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to delete route");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting route");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the route");
                throw new Exception("An error occurred while deleting the route", ex);
            }
        }



        public async Task<OneRoute> GetByIdAsync(Guid userId, string userRole, Guid routeId)
        {
            try
            {
                // Fetch the route
                var route = await _context.Route.FindAsync(routeId);
                if (route == null) throw new Exception("Route not found");

                // Validate permissions
                if (userRole == "2") // Entity Admin
                {
                    // Check if the user is linked to the route's entity as an EntityDriver
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == route.EntityId);
                    if (!isEntityDriver)
                        throw new Utils.UnauthorizedAccessException("User is not authorized to view this route.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new Utils.UnauthorizedAccessException("User is not authorized to view routes.");
                }

                // Map the route to the DTO and return it
                return _mapper.Map<OneRoute>(route);
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to get route by ID");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the route");
                throw new Exception("An error occurred while retrieving the route", ex);
            }
        }


        public async Task<OneRoute> GetByIdMobileAsync(Guid id, Guid loggedUser)
        {
            try
            {
                var route = await _context.Route.FindAsync(id);
                if (route == null) throw new Exception("Route not found");

                // Automatically create a RouteHistory entry for the user

                if (loggedUser != Guid.Empty)
                {
                    _logger.LogInformation($"Creating RouteHistory entry for UserId: {loggedUser} and RouteId: {id}");

                    await _routeHistoryServices.CreateRouteHistoryAsync(new Requests.RouteHistory.PostRouteHistory
                    {
                        UserId = loggedUser,
                        RouteId = id
                    });

                    _logger.LogInformation("RouteHistory entry created successfully.");
                }
                else
                {
                    _logger.LogWarning("User ID is empty, skipping RouteHistory creation.");
                }

                return _mapper.Map<OneRoute>(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding the route or creating RouteHistory.");
                throw new Exception("An error occurred while finding the route or creating RouteHistory", ex);
            }
        }

        public async Task<List<ListRoute>> GetRoutesAsync(FilterQuery? filter)
        {
            try
            {
                var prepRoutes = _context.Route.ProjectTo<ListRoute>(_mapper.ConfigurationProvider);
                if (prepRoutes == null) throw new Exception("Error getting routes");

                QueryHelper.ApplyListFilters(ref prepRoutes, filter);

                var routes = await prepRoutes.ToListAsync();
                if (routes == null || routes.Count == 0) return new List<ListRoute>();

                return routes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting routes");
                throw new Exception("An error occurred while getting routes", ex);
            }
        }

        public async Task UpdateRouteAsync(Guid userId, string userRole, Guid routeId, PatchRoute request)
        {
            try
            {
                // Fetch the existing route
                var oldRoute = await _context.Route.FindAsync(routeId);
                if (oldRoute == null) throw new Exception("Route not found");

                // Validate permissions
                if (userRole == "2") // Entity Admin
                {
                    // Check if the user is linked to the route's entity as an EntityDriver
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == oldRoute.EntityId);
                    if (!isEntityDriver) throw new Utils.UnauthorizedAccessException("User is not authorized to update this route.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new Utils.UnauthorizedAccessException("User is not authorized to update routes.");
                }

                // Validate new RegionId, if provided
                if (request.RegionId.HasValue)
                {
                    var regionExists = await _context.Region.AnyAsync(r => r.Id == request.RegionId);
                    if (!regionExists) throw new Exception("Region not found");
                }

                // Validate new EntityId, if provided
                if (request.EntityId.HasValue)
                {
                    var entityExists = await _context.Entity.AnyAsync(e => e.Id == request.EntityId);
                    if (!entityExists) throw new Exception("Entity not found");

                    // For Entity Admin, ensure they are allowed to switch to the new entity
                    if (userRole == "2")
                    {
                        var isEntityDriverForNewEntity = await _context.EntityDriver
                            .AnyAsync(ed => ed.UserId == userId && ed.EntityId == request.EntityId.Value);
                        if (!isEntityDriverForNewEntity)
                            throw new Utils.UnauthorizedAccessException("User is not authorized to assign this route to the new entity.");
                    }
                }

                // Update the route fields
                oldRoute.Name = request.Name ?? oldRoute.Name;
                oldRoute.EntityId = request.EntityId ?? oldRoute.EntityId;
                oldRoute.Active = request.Active ?? oldRoute.Active;
                oldRoute.RegionId = request.RegionId ?? oldRoute.RegionId;
                if(userRole == "3") oldRoute.EntityId = request.EntityId ?? oldRoute.EntityId;

                // Save changes
                await _context.SaveChangesAsync();
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to update route");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the route");
                throw new Exception("An error occurred while updating the route", ex);
            }
        }

    }
}
