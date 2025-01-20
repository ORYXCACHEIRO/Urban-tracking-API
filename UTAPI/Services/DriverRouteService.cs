using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.DriverRoute;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class DriverRouteServices : IDriverRouteServices
    {
        private readonly DataContext _context;
        private readonly ILogger<DriverRouteServices> _logger;
        private readonly IMapper _mapper;

        public DriverRouteServices(DataContext context, IMapper mapper, ILogger<DriverRouteServices> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ListDriverRoute> CreateDriverRouteAsync(PostDriverRoute request, Guid loggedUserId, string userRole)
        {
            try
            {
                // Validate that the user being inserted is a valid driver (role = 1)
                var userExists = await _context.User.AnyAsync(u => u.Id == request.UserId && u.Role == "1"); // Role 1 = Driver
                if (!userExists)
                {
                    throw new Exception("The user must be a valid driver (role = 1)");
                }

                // Validate if the route exists and retrieve its EntityId
                var route = await _context.Route.FirstOrDefaultAsync(r => r.Id == request.RouteId);
                if (route == null)
                {
                    throw new Exception("Route not found");
                }

                // Check if a record with the same UserId and RouteId already exists
                var existingDriverRoute = await _context.DriverRoute
                    .AnyAsync(dr => dr.UserId == request.UserId && dr.RouteId == request.RouteId);
                if (existingDriverRoute)
                {
                    throw new Exception("A driver route with the same UserId and RouteId already exists.");
                }

                // If the user is an entity admin (role = 2)
                if (userRole == "2") // Entity Admin
                {
                    // Check if the route's EntityId matches the entity associated with the logged-in user (admin)
                    var adminEntityDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId && ed.EntityId == route.EntityId);

                    if (adminEntityDriver == null)
                    {
                        throw new Exception("The logged-in user must be an entity admin for the same entity as the route");
                    }

                    // Validate that the user being inserted is an entity driver for the same entity as the entity admin
                    var userEntityDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == request.UserId && ed.EntityId == route.EntityId);

                    if (userEntityDriver == null)
                    {
                        throw new Exception("The user must be an entity driver for the same entity as the entity admin");
                    }
                }
                else if (userRole != "3") // Not a Sys Admin and not an Entity Admin
                {
                    throw new Exception("Unauthorized action. Only Entity Admins or Sys Admins can create driver routes");
                }

                // Validate that workDays is a 7-digit string containing only 0s and 1s
                if (request.WorkDays.Length != 7 || !request.WorkDays.All(c => c == '0' || c == '1'))
                {
                    throw new Exception("WorkDays must be a 7-digit string containing only '0' or '1'");
                }

                // Proceed with creating the DriverRoute
                var driverRoute = _mapper.Map<DriverRoute>(request);
                _context.DriverRoute.Add(driverRoute);
                await _context.SaveChangesAsync();

                // Return the created record as ListDriverRoute
                var driverRouteResponse = _mapper.Map<ListDriverRoute>(driverRoute);
                return driverRouteResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the driver route");
                throw new Exception("An error occurred while creating the driver route", ex);
            }
        }


        public async Task<List<ListDriverRoute>> GetDriverRoutesByUserAsync(Guid userId, FilterQuery? filter, string userRole, Guid loggedUserId)
        {
            try
            {
                IQueryable<DriverRoute> prepDriverRoutes;

                // Check if the logged-in user is an Entity Admin
                if (userRole == "2") // Entity Admin
                {
                    // Retrieve the EntityId for the logged-in user from the EntityDriver table
                    var entityAdmin = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (entityAdmin == null)
                    {
                        throw new Exception("The logged-in user must be an entity admin");
                    }

                    // Retrieve the driver routes for the user associated with the same entity as the entity admin
                    prepDriverRoutes = _context.DriverRoute
                        .Where(dr => dr.UserId == userId) // Apply userId filter
                        .Where(dr => _context.EntityDriver.Any(ed => ed.UserId == dr.UserId && ed.EntityId == entityAdmin.EntityId)); // Apply EntityId filter based on the entity admin
                }
                else if(userRole == "1")
                {
                    if(userId != loggedUserId) throw new Exception("The logged-in does not have permissions to request this informartion");

                    var isDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (isDriver == null)
                    {
                        throw new Exception("The logged-in user must be a driver for an entity");
                    }

                    prepDriverRoutes = _context.DriverRoute
                        .Where(dr => dr.UserId == loggedUserId);
                }
                else
                {
                    // If the user is not an Entity Admin, return all routes for the user without entity-based restrictions
                    prepDriverRoutes = _context.DriverRoute
                        .Where(dr => dr.UserId == userId);
                }

                // Apply additional filters if provided
                if (filter != null)
                {
                    QueryHelper.ApplyListFiltersWithoutVars(ref prepDriverRoutes, filter, ["UserId"]);
                }

                // Map the result to the ListDriverRoute response
                var driverRoutes = await prepDriverRoutes
                    .ProjectTo<ListDriverRoute>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                if (driverRoutes == null || driverRoutes.Count == 0)
                {
                    return new List<ListDriverRoute>();
                }

                return driverRoutes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving driver routes");
                throw new Exception("An error occurred while retrieving driver routes", ex);
            }
        }

        public async Task<ListDriverRoute> GetDriverRouteByIdAsync(Guid driverRouteId, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the driver route by its ID
                var driverRoute = await _context.DriverRoute
                    .FirstOrDefaultAsync(dr => dr.Id == driverRouteId);

                if (driverRoute == null)
                {
                    throw new Exception("Driver route not found");
                }

                // Check if the user is an entity admin (role = "2")
                if (userRole == "2") // Entity Admin
                {
                    // Fetch the entity associated with the user (the entity admin)
                    var userEntityDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (userEntityDriver == null)
                    {
                        throw new Exception("Entity admin can only access routes for their own entity");
                    }

                    // Now, check if the route belongs to the same entity the admin manages.
                    // Assuming the `driverRoute` is associated with an entity via a Route table.
                    var route = await _context.Route
                        .FirstOrDefaultAsync(r => r.Id == driverRoute.RouteId);  // Assuming there's a RouteId in driverRoute

                    if (route == null)
                    {
                        throw new Exception("Route not found");
                    }

                    // Check if the EntityId associated with the route matches the admin's entity.
                    if (route.EntityId != userEntityDriver.EntityId)
                    {
                        throw new Exception("Entity admin can only access routes for their own entity");
                    }
                }
                else if (userRole == "1") // Driver
                {
                    // If the user is a driver, they can only access their own route
                    if (driverRoute.UserId != loggedUserId)
                    {
                        throw new Exception("You can only access your own driver route");
                    }
                }
                else if (userRole != "3") throw new Utils.UnauthorizedAccessException("User is not authorized to delete drive route");

                // Map and return the driver route
                return _mapper.Map<ListDriverRoute>(driverRoute);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the driver route");
                throw new Exception("An error occurred while retrieving the driver route", ex);
            }
        }

        public async Task<List<ListDriverRoute>> GetDriverRoutesAsync(string userRole, FilterQuery? filter)
        {
            try
            {
                // Validate user role
                if (userRole != "3")
                    throw new Exception("This user does not have permission to use this service");

                // Prepare query for driver routes
                var prepDriverRoutes = _context.DriverRoute
                    .ProjectTo<ListDriverRoute>(_mapper.ConfigurationProvider);

                if (prepDriverRoutes == null)
                    throw new Exception("Error getting driver routes");

                // Apply filters to the query
                QueryHelper.ApplyListFilters(ref prepDriverRoutes, filter);

                // Execute the query and retrieve results
                var driverRoutes = await prepDriverRoutes.ToListAsync();

                // Return an empty list if no routes are found
                if (driverRoutes == null || driverRoutes.Count == 0)
                    return new List<ListDriverRoute>();

                return driverRoutes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the driver routes");
                throw new Exception("An error occurred while retrieving the driver routes", ex);
            }
        }

        public async Task<List<ListDriverRoute>> GetDriverRoutesByRouteIdAsync(Guid routeId, Guid loggedUserId, string userRole, FilterQuery? filter)
        {
            try
            {
                // Check if the route exists
                var route = await _context.Route.FirstOrDefaultAsync(r => r.Id == routeId);
                if (route == null)
                    throw new Exception("The specified route does not exist");

                IQueryable<DriverRoute> prepDriverRoutes;

                if (userRole == "2") // Entity Admin
                {
                    // Check if the logged-in user is associated with the route's EntityId via EntityDriver
                    var entityDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId && ed.EntityId == route.EntityId);

                    if (entityDriver == null)
                        throw new Exception("The logged-in user is not authorized to view driver routes for this entity");

                    // Fetch driver routes linked to the specified routeId
                    prepDriverRoutes = _context.DriverRoute
                        .Where(dr => dr.RouteId == routeId)
                        .Include(dr => dr.User); // Include related user data
                }
                else if (userRole == "3") // Sys Admin
                {
                    // Sys Admin can fetch any driver routes linked to the specified routeId
                    prepDriverRoutes = _context.DriverRoute
                        .Where(dr => dr.RouteId == routeId)
                        .Include(dr => dr.User); // Include related user data
                }
                else
                {
                    throw new Exception("Unauthorized action. Only Entity Admins or Sys Admins can view driver routes by routeId");
                }

                // Apply filters if provided
                if (filter != null)
                {
                    QueryHelper.ApplyListFilters(ref prepDriverRoutes, filter);
                }

                // Map to DTO
                var mappedDriverRoutes = prepDriverRoutes.ProjectTo<ListDriverRoute>(_mapper.ConfigurationProvider);

                // Execute query and retrieve results
                var driverRoutes = await mappedDriverRoutes.ToListAsync();

                // Return an empty list if no driver routes are found
                if (driverRoutes == null || driverRoutes.Count == 0)
                    return new List<ListDriverRoute>();

                return driverRoutes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving driver routes by routeId");
                throw new Exception("An error occurred while retrieving driver routes by routeId", ex);
            }
        }


        public async Task DeleteDriverRouteAsync(Guid driverRouteId, Guid loggedUserId, string userRole)
        {
            try
            {
                // Fetch the driver route by its ID
                var driverRoute = await _context.DriverRoute
                    .FirstOrDefaultAsync(dr => dr.Id == driverRouteId);

                if (driverRoute == null)
                {
                    throw new Exception("Driver route not found");
                }

                // Check if the user is an entity admin (role = "2")
                if (userRole == "2") // Entity Admin
                {
                    // Fetch the entity associated with the user (the entity admin)
                    var userEntityDriver = await _context.EntityDriver
                        .FirstOrDefaultAsync(ed => ed.UserId == loggedUserId);

                    if (userEntityDriver == null)
                    {
                        throw new Exception("Entity admin can only delete routes for their own entity");
                    }

                    // Now, check if the route belongs to the same entity the admin manages.
                    var route = await _context.Route
                        .FirstOrDefaultAsync(r => r.Id == driverRoute.RouteId); // Assuming there's a RouteId in driverRoute

                    if (route == null)
                    {
                        throw new Exception("Route not found");
                    }

                    // Check if the EntityId associated with the route matches the admin's entity.
                    if (route.EntityId != userEntityDriver.EntityId)
                    {
                        throw new Exception("Entity admin can only delete routes for their own entity");
                    }
                }
                else if(userRole != "3") throw new Utils.UnauthorizedAccessException("User is not authorized to delete drive route");

                // Delete the driver route
                _context.DriverRoute.Remove(driverRoute);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the driver route");
                throw new Exception("An error occurred while deleting the driver route", ex);
            }
        }

    }
}
