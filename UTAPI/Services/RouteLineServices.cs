using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Models;
using UTAPI.Interfaces;
using UTAPI.Requests.RouteLine;
using AutoMapper.QueryableExtensions;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Services
{
    public class RouteLineServices : IRouteLineServices
    {
        private readonly DataContext _context;
        private readonly ILogger<RouteLineServices> _logger;
        private readonly IMapper _mapper;

        public RouteLineServices(DataContext context, ILogger<RouteLineServices> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // Criação de uma nova linha de rota
        public async Task<RouteLine> CreateRouteLineAsync(Guid userId, string userRole, PostRouteLine request)
        {
            try
            {
                // Check if the route exists
                var route = await _context.Route.FindAsync(request.RouteId);
                if (route == null) throw new Exception("Route not found");

                // Permission check for Entity Admin (role = "2")
                if (userRole == "2") // Entity Admin
                {
                    // Check if the route is associated with the user's entity
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == route.EntityId);
                    if (!isEntityDriver)
                        throw new Utils.UnauthorizedAccessException("User is not authorized to create RouteLine for this Route.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new Utils.UnauthorizedAccessException("User is not authorized to create RouteLine associations.");
                }

                // Map the request to a RouteLine object
                var routeLine = _mapper.Map<RouteLine>(request);

                // Add RouteLine to the context
                _context.RouteLine.Add(routeLine);
                await _context.SaveChangesAsync();

                return routeLine;
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to create a RouteLine");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the route line");
                throw new Exception("An error occurred while creating the route line", ex);
            }
        }


        // Exclusão de uma linha de rota
        public async Task DeleteRouteLineAsync(Guid userId, string userRole, Guid id)
        {
            try
            {
                // Retrieve the RouteLine based on the given id
                var routeLine = await _context.RouteLine.FirstOrDefaultAsync(rl => rl.Id == id);
                if (routeLine == null) throw new Exception("Route line not found");

                // Retrieve the Route associated with the RouteLine
                var route = await _context.Route.FirstOrDefaultAsync(r => r.Id == routeLine.RouteId);
                if (route == null) throw new Exception("Route not found");

                // Permission check for Entity Admin (role = "2")
                if (userRole == "2") // Entity Admin
                {
                    // Check if the route is associated with the user's entity by querying the EntityDriver table
                    var isEntityDriver = await _context.EntityDriver
                        .AnyAsync(ed => ed.UserId == userId && ed.EntityId == route.EntityId);
                    if (!isEntityDriver)
                        throw new Utils.UnauthorizedAccessException("User is not authorized to delete this RouteLine.");
                }
                else if (userRole != "3") // Not System Admin
                {
                    throw new Utils.UnauthorizedAccessException("User is not authorized to delete RouteLine associations.");
                }

                // Delete the route line from the database
                _context.RouteLine.Remove(routeLine);
                await _context.SaveChangesAsync();
            }
            catch (Utils.UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to delete a RouteLine");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the route line");
                throw new Exception("An error occurred while deleting the route line", ex);
            }
        }


        // Buscar uma linha de rota por Id
        public async Task<OneRouteLine> GetByIdAsync(Guid id)
        {
            try
            {
                var routeLine = await _context.RouteLine
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (routeLine == null) throw new Exception("Route line not found");

                var routeLineResponse = _mapper.Map<OneRouteLine>(routeLine);
                return routeLineResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding the route line");
                throw new Exception("An error occurred while finding the route line");
            }
        }

        // Buscar todas as linhas de rota por ROUTEId
        public async Task<List<ListRouteLine>> GetByRouteIdAsync(Guid routeId, FilterQuery? filter)
        {
            try
            {
                // Query the RouteLine table, filtering by RouteId
                var prepRouteLines = _context.RouteLine
                    .Where(rl => rl.RouteId == routeId)
                    .ProjectTo<ListRouteLine>(_mapper.ConfigurationProvider);

                // Apply additional filters if provided
                QueryHelper.ApplyListFiltersWithoutVars(ref prepRouteLines, filter, ["RouteId"]);

                // Execute the query and fetch the results as a list
                var routeLines = await prepRouteLines.ToListAsync();

                if (routeLines == null || routeLines.Count == 0) return new List<ListRouteLine>();

                return routeLines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding route lines");
                throw new Exception("An error occurred while finding route lines", ex);
            }
        }

        //Func que permite pesquisar route lines
        public async Task<List<ListRouteLine>> GetRouteLinesAsync(FilterQuery? filter)
        {
            var prepRouteLines = _context.RouteLine
                .ProjectTo<ListRouteLine>(_mapper.ConfigurationProvider);

            if (prepRouteLines == null) throw new Exception("Error getting route lines");

            try
            {
                QueryHelper.ApplyListFilters(ref prepRouteLines, filter);

                var routeLines = await prepRouteLines.ToListAsync();

                if (routeLines == null || routeLines.Count == 0) return new List<ListRouteLine>();

                return routeLines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding route lines");
                throw new Exception("An error occurred while finding route lines", ex);
            }
        }


        // Listar todas as linhas de rota
        public async Task<List<ListRouteLine>> GetRouteLinesForEntityAdminAsync(Guid userId, FilterQuery? filter)
        {
            try
            {
                // Get all RouteIds associated with the user's entity
                var routeIdsForEntity = await _context.EntityDriver
                    .Where(ed => ed.UserId == userId)
                    .Select(ed => ed.EntityId)
                    .Distinct()
                    .ToListAsync();

                if (!routeIdsForEntity.Any())
                    throw new Utils.UnauthorizedAccessException("User is not associated with any routes.");

                // Filter RouteLines based on the user's entity's routes
                var prepRouteLines = _context.RouteLine
                    .Where(rl => _context.Route
                        .Where(r => routeIdsForEntity.Contains(r.EntityId))  // Filter routes by the user's entity
                        .Select(r => r.Id)  // Get the RouteIds for those routes
                        .Contains(rl.RouteId))  // Ensure the RouteLine is for one of the user's routes
                    .ProjectTo<ListRouteLine>(_mapper.ConfigurationProvider);

                // Apply additional filters if provided
                QueryHelper.ApplyListFiltersWithoutVars(ref prepRouteLines, filter, ["RouteId"]);

                var routeLines = await prepRouteLines.ToListAsync();

                return routeLines ?? new List<ListRouteLine>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving route lines.");
                throw new Exception("An error occurred while retrieving route lines.", ex);
            }
        }

    }
}
