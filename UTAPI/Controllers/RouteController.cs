using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Requests.Route;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing Routes.
    /// Supports creating, retrieving, updating, and deleting routes.
    /// To enable XML support, ensure that `AddXmlSerializerFormatters()` is added in the application's service configuration.
    /// The response format (JSON or XML) will depend on the `Accept` header in client requests.
    /// </summary>
    [Route("api/v1/route")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly IRouteServices _routeServices;

        public RouteController(IRouteServices routeServices)
        {
            _routeServices = routeServices;
        }

        /// <summary>
        /// Creates a new route.
        /// Content negotiation will allow this endpoint to return XML or JSON based on the client's `Accept` header.
        /// </summary>
        /// <param name="request">The route details to create.</param>
        /// <returns>The created route details.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<IActionResult> CreateRouteAsync(PostRoute request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var route = await _routeServices.CreateRouteAsync(userId, roleClaim, request);
                    return Ok(route); // Response will be formatted as JSON or XML based on `Accept` header.
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
        /// Retrieves all routes with optional filtering.
        /// The client can specify `Accept: application/xml` to receive the response in XML format.
        /// </summary>
        /// <param name="filter">Filter parameters for the routes.</param>
        /// <returns>List of routes.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ListRoute>> GetAllRoutes(FilterQuery filter)
        {
            try
            {
                var routes = await _routeServices.GetRoutesAsync(filter);

                Response.Headers["X-Count"] = routes.Count.ToString();

                return Ok(routes); // Supports XML or JSON response based on `Accept` header.
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a route by its ID.
        /// Automatically supports content negotiation for JSON or XML.
        /// </summary>
        /// <param name="id">The ID of the route to retrieve.</param>
        /// <returns>The route details.</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OneRoute>> GetRoute(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    switch (roleClaim)
                    {
                        case Roles.User:
                        case Roles.Driver:
                            var mobileRoute = await _routeServices.GetByIdMobileAsync(id, userId);
                            if (mobileRoute == null) return NotFound(new { message = "Route not found" });
                            return Ok(mobileRoute);
                        default:
                            var route = await _routeServices.GetByIdAsync(userId, roleClaim, id);
                            if (route == null) return NotFound(new { message = "Route not found" });
                            return Ok(route);
                    }
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
        /// Updates an existing route by its ID.
        /// Supports XML responses if requested by the client.
        /// </summary>
        /// <param name="request">The updated route details.</param>
        /// <param name="id">The ID of the route to update.</param>
        /// <returns>Success message.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRouteAsync(PatchRoute request, Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _routeServices.UpdateRouteAsync(userId, roleClaim, id, request);
                    return Ok(new { message = "Route successfully updated" });
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
        /// Deletes a route by its ID.
        /// Supports XML responses if requested by the client.
        /// </summary>
        /// <param name="id">The ID of the route to delete.</param>
        /// <returns>Success message.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRouteAsync(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _routeServices.DeleteRouteAsync(userId, roleClaim, id);
                    return Ok(new { message = "Route successfully deleted" });
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
    }
}
