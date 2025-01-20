using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UTAPI.Models;
using UTAPI.Requests.RouteStop;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;
using UTAPI.Interfaces;
using System.Security.Claims;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing route stops.
    /// Supports adding, deleting, and retrieving route stop associations based on user roles.
    /// Content negotiation allows the response to be in either JSON or XML format based on the `Accept` header.
    /// </summary>
    [Route("api/v1/routeStop")]
    [ApiController]
    public class RouteStopController : ControllerBase
    {
        private readonly IRouteStopServices _routeStopServices;

        // Constructor now takes IRouteStopServices instead of RouteStopServices
        public RouteStopController(IRouteStopServices routeStopServices)
        {
            _routeStopServices = routeStopServices;
        }

        /// <summary>
        /// Adds a stop to a specific route.
        /// Accessible to users with Admin or Entity Admin roles.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="request">The request object containing the route stop details.</param>
        /// <returns>The newly created route stop association.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost("add-to-route")]
        public async Task<IActionResult> AddStopToRouteAsync([FromBody] PostRouteStop request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    // Call the service method to create the route-stop association
                    var routeStop = await _routeStopServices.CreateRouteStopAsync(userId, roleClaim, request);
                    return Ok(routeStop); // Response formatted based on `Accept` header
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific route stop association by ID.
        /// Accessible to users with Admin or Entity Admin roles.
        /// </summary>
        /// <param name="routeStopId">The ID of the route stop to be deleted.</param>
        /// <returns>A success or error message indicating whether the route stop was deleted.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{routeStopId}")]
        public async Task<IActionResult> DeleteRouteStopAsync(Guid routeStopId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    // Call the service method to delete the route-stop association
                    await _routeStopServices.DeleteRouteStopAsync(userId, roleClaim, routeStopId);
                    return Ok(new { message = "RouteStop association deleted successfully" }); // Response formatted based on `Accept` header
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all route stops associated with a specific route.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="routeId">The ID of the route whose stops are being retrieved.</param>
        /// <returns>The list of route stops associated with the specified route.</returns>
        [Authorize]
        [HttpGet("by-route/{routeId}")]
        public async Task<IActionResult> GetRouteStopsByRoute(Guid routeId)
        {
            try
            {
                var routeStops = await _routeStopServices.GetRouteStopsByRouteIdAsync(routeId);
                Response.Headers["X-Count"] = routeStops.Count.ToString();
                return Ok(routeStops); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of route stops based on optional query filters.
        /// Accessible to users with Admin or Entity Admin roles.
        /// </summary>
        /// <param name="filter">Optional query parameters for filtering the results.</param>
        /// <returns>The list of route stops based on the specified filters.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpGet]
        public async Task<ActionResult<List<RouteStop>>> ListRouteStops([FromQuery] FilterQuery filter)
        {
            try
            {
                // Optionally validate filter query
                if (filter.Limit < 1)
                {
                    return BadRequest(new { message = "Limit must be a positive number." });
                }

                // Get the list of route stops
                var routeStops = await _routeStopServices.GetRouteStopsAsync(filter);
                Response.Headers["X-Count"] = routeStops.Count.ToString();

                // Return the list of route stops with 200 OK status
                return Ok(routeStops); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error with a message
                return StatusCode(500, new { message = "An error occurred while retrieving route stops.", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific route stop by its ID.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="id">The ID of the route stop to retrieve.</param>
        /// <returns>The details of the route stop with the specified ID.</returns>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRouteStopById(Guid id)
        {
            try
            {
                var routeStop = await _routeStopServices.GetByIdAsync(id);
                return Ok(routeStop); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
