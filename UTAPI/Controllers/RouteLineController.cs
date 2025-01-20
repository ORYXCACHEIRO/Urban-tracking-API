using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.RouteLine;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing route lines.
    /// Supports creation, retrieval, and deletion of route lines for specific users, with role-based access control.
    /// Content negotiation allows the response to be in either JSON or XML format based on the `Accept` header.
    /// </summary>
    [Route("api/v1/routeline")]
    [ApiController]
    public class RouteLineController : ControllerBase
    {
        private readonly IRouteLineServices _routeLineServices;

        public RouteLineController(IRouteLineServices routeLineServices)
        {
            _routeLineServices = routeLineServices;
        }

        /// <summary>
        /// Creates a new route line.
        /// Accessible to users with Admin or Entity Admin roles.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="request">The request object containing details for the new route line.</param>
        /// <returns>The created route line.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<IActionResult> CreateRouteLineAsync(PostRouteLine request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var routeLine = await _routeLineServices.CreateRouteLineAsync(userId, roleClaim, request);
                    return Ok(routeLine); // Response formatted based on `Accept` header
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
        /// Retrieves all route lines.
        /// Role-based access: Admin can view all route lines; Entity Admin can view only their entity's route lines.
        /// </summary>
        /// <param name="filter">Optional query parameters for filtering the results.</param>
        /// <returns>The list of route lines based on the user's role.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpGet]
        public async Task<ActionResult<List<ListRouteLine>>> GetAllRouteLines([FromQuery] FilterQuery? filter)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    switch (roleClaim)
                    {
                        case Roles.Admin:
                            var rl1 = await _routeLineServices.GetRouteLinesAsync(filter);
                            Response.Headers["X-Count"] = rl1.Count.ToString();
                            return Ok(rl1); // Response formatted based on `Accept` header
                        case Roles.AdminEnt:
                            var rl2 = await _routeLineServices.GetRouteLinesForEntityAdminAsync(userId, filter);
                            Response.Headers["X-Count"] = rl2.Count.ToString();
                            return Ok(rl2); // Response formatted based on `Accept` header
                        default:
                            return StatusCode(404, new { message = "Error getting route lines" });
                    }
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
        /// Retrieves all route lines associated with a specific route ID.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="filter">Optional query parameters for filtering the results.</param>
        /// <param name="routeId">The ID of the route whose route lines are being retrieved.</param>
        /// <returns>The list of route lines for the specified route ID.</returns>
        [Authorize]
        [HttpGet("by-route/{routeId}")]
        public async Task<ActionResult<List<ListRouteLine>>> GetAllRouteLinesByRoute([FromQuery] FilterQuery? filter, Guid routeId)
        {
            try
            {
                var routeLines = await _routeLineServices.GetByRouteIdAsync(routeId, filter);
                Response.Headers["X-Count"] = routeLines.Count.ToString();
                return Ok(routeLines); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific route line by ID.
        /// Accessible to users with Admin or Entity Admin roles.
        /// </summary>
        /// <param name="id">The ID of the route line to be deleted.</param>
        /// <returns>A success or error message indicating whether the route line was deleted.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRouteLineAsync(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _routeLineServices.DeleteRouteLineAsync(userId, roleClaim, id);
                    return Ok(new { message = "RouteLine successfully deleted" }); // Response formatted based on `Accept` header
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
