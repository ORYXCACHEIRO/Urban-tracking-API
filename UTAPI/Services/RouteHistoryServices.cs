using Microsoft.EntityFrameworkCore;
using UTAPI.Data;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.RouteHistory;

namespace UTAPI.Services
{
    public class RouteHistoryServices : IRouteHistoryServices
    {
        private readonly DataContext _context;

        public RouteHistoryServices(DataContext context)
        {
            _context = context;
        }

        // Create a new route history entry
        public async Task<RouteHistory> CreateRouteHistoryAsync(PostRouteHistory request)
        {
            var routeHistory = new RouteHistory
            {
                UserId = request.UserId,
                RouteId = request.RouteId
            };

            _context.RouteHistory.Add(routeHistory);
            await _context.SaveChangesAsync();

            return routeHistory;
        }

        // Retrieve route history for a specific user
        public async Task<List<RouteHistory>> GetRouteHistoryByUserIdAsync(Guid userId, Guid loggedUserId)
        {
            if (loggedUserId != userId)
                return new List<RouteHistory>(); // Return an empty list if the IDs don't match

            return await _context.RouteHistory
                .Where(rh => rh.UserId == userId)
                .ToListAsync();
        }


        // Delete a specific route history entry based on UserId and RouteId
        public async Task<bool> DeleteRouteHistoryAsync(Guid userId, Guid routeId, Guid loggedUserId)
        {
            if (loggedUserId != userId)
            {
                return false; // Unauthorized: Logged-in user doesn't match the provided userId
            }

            var routeHistory = await _context.RouteHistory
                .FirstOrDefaultAsync(rh => rh.UserId == userId && rh.RouteId == routeId);

            if (routeHistory == null)
            {
                return false; // Entry not found
            }

            _context.RouteHistory.Remove(routeHistory);
            await _context.SaveChangesAsync();

            return true; // Deletion was successful
        }

    }
}
