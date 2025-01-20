using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Interfaces;
using AutoMapper.QueryableExtensions;
using UTAPI.Requests.FavRoute;
using UTAPI.Models;

namespace UTAPI.Services
{
    public class FavRouteServices : IFavRouteServices
    {
        private readonly DataContext _context;
        private readonly ILogger<FavRouteServices> _logger;
        private readonly IMapper _mapper;

        public FavRouteServices(DataContext context, ILogger<FavRouteServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<FavRoute> CreateFavRouteAsync(PostFavRoute request, Guid loggedUserId)
        {
            try
            {
                // Validate that the provided UserId matches the logged-in user's ID
                if (request.UserId != loggedUserId)
                {
                    throw new Exception("User ID does not match the logged-in user.");
                }

                // Check if the user exists
                var userExists = await _context.User.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    throw new Exception("User not found");
                }

                // Check if the route exists
                var routeExists = await _context.Route.AnyAsync(r => r.Id == request.RouteId);
                if (!routeExists)
                {
                    throw new Exception("Route not found");
                }

                // Check if the favorite route already exists for the same UserId and RouteId
                var favRouteExists = await _context.FavRoute
                    .AnyAsync(fr => fr.UserId == request.UserId && fr.RouteId == request.RouteId);

                if (favRouteExists)
                {
                    throw new Exception("This route is already marked as a favorite for the user.");
                }

                // Map the request to the FavRoute model and add it
                var favRoute = _mapper.Map<FavRoute>(request);
                _context.FavRoute.Add(favRoute);

                // Save changes to the database
                await _context.SaveChangesAsync();

                return favRoute;
            }
            catch (Exception ex)
            {
                // Log the error and throw a new exception
                _logger.LogError(ex, "An error occurred while adding a favorite route");
                throw new Exception("An error occurred while adding a favorite route", ex);
            }
        }


        public async Task DeleteFavRouteAsync(Guid id, Guid loggedUserId)
        {
            try
            {
                // Find the favorite route by ID
                var favRoute = await _context.FavRoute
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (favRoute == null)
                {
                    throw new Exception("Favorite Route not found");
                }

                // Verify that the loggedUserId matches the UserId associated with the favorite route
                if (favRoute.UserId != loggedUserId)
                {
                    throw new Exception("You do not have permission to delete this favorite route.");
                }

                // Remove the favorite route from the database
                _context.FavRoute.Remove(favRoute);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the route");
                throw new Exception("An error occurred while deleting the route", ex);
            }
        }

        public async Task<List<GetFavRouteByUserId>> GetFavRouteByUserIdAsync(Guid userId, Guid loggedUserId)
        {
            try
            {
                // Check if the loggedUserId matches the userId provided
                if (userId != loggedUserId)
                {
                    throw new Exception("You do not have permission to view the routes for this user.");
                }

                var routes = await _context.FavRoute
                    .Where(r => r.UserId == userId)
                    .ProjectTo<GetFavRouteByUserId>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                if (routes == null || routes.Count == 0) return new List<GetFavRouteByUserId>();

                return routes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the routes");
                throw new Exception("An error occurred while retrieving the routes", ex);
            }
        }

    }
}
