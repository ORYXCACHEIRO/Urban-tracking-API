using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Models;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing route history.
    /// Supports retrieving and deleting route history entries for specific users.
    /// Content negotiation allows the response to be in either JSON or XML format based on the `Accept` header.
    /// </summary>
    [Route("api/v1/routeHistory")]
    [ApiController]
    public class RouteHistoryController : ControllerBase
    {
        private readonly IRouteHistoryServices _routeHistoryServices;

        public RouteHistoryController(IRouteHistoryServices routeHistoryServices)
        {
            _routeHistoryServices = routeHistoryServices;
        }

        /// <summary>
        /// Retrieves the route history for a specific user.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="userId">The ID of the user for which route history is to be retrieved.</param>
        /// <returns>The route history for the specified user.</returns>
        [Authorize]
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetRouteHistoryByUserId(Guid userId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var loggedUser) && roleClaim != null)
                {
                    var routeHistory = await _routeHistoryServices.GetRouteHistoryByUserIdAsync(userId, loggedUser);
                    return Ok(routeHistory); // Response formatted based on `Accept` header
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific route history entry based on the UserId and RouteId.
        /// Supports content negotiation to return results in JSON or XML format.
        /// </summary>
        /// <param name="userId">The ID of the user whose route history entry is to be deleted.</param>
        /// <param name="routeId">The ID of the route history entry to be deleted.</param>
        /// <returns>A success or error message indicating whether the route history entry was deleted.</returns>
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteRouteHistoryAsync([FromQuery] Guid userId, [FromQuery] Guid routeId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var loggedUser) && roleClaim != null)
                {
                    var result = await _routeHistoryServices.DeleteRouteHistoryAsync(userId, routeId, loggedUser);

                    if (result)
                        return Ok(new { message = "Route history entry deleted successfully" }); // Response formatted based on `Accept` header

                    return NotFound(new { message = "Route history entry not found" });
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
