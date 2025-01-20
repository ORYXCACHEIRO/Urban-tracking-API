using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.DriverRoute;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for handling Driver-Route related operations.
    /// Supports creating, fetching, and deleting driver routes.
    /// </summary>
    [Route("api/v1/driverRoute")]
    [ApiController]
    public class DriverRouteController : ControllerBase
    {
        private readonly IDriverRouteServices _driverRouteServices;

        /// <summary>
        /// Constructor to initialize DriverRouteController with required services.
        /// </summary>
        public DriverRouteController(IDriverRouteServices driverRouteServices)
        {
            _driverRouteServices = driverRouteServices;
        }

        /// <summary>
        /// Creates a new driver-route association.
        /// Requires Admin or AdminEnt role to access.
        /// </summary>
        /// <param name="request">Driver route creation details.</param>
        /// <returns>Returns the created driver route details.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<IActionResult> CreateDriverRouteAsync(PostDriverRoute request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState); // Return 400 if model is invalid

            try
            {
                // Retrieve 'sub' and 'role' from the authenticated user's claims
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid (user ID)
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    // Call service to create the driver route
                    var driverRoute = await _driverRouteServices.CreateDriverRouteAsync(request, userId, roleClaim);
                    return Ok(driverRoute); // Return created driver route
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." }); // Return 404 if 'sub' is not a valid Guid
                }
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message }); // Return 404 for unexpected exceptions
            }
        }

        /// <summary>
        /// Retrieves a list of driver routes associated with a specific user.
        /// Accessible by Admin, AdminEnt, and Driver roles.
        /// </summary>
        /// <param name="userId">The ID of the user whose driver routes are to be retrieved.</param>
        /// <param name="filter">Optional filter query parameters for pagination or other criteria.</param>
        /// <returns>Returns a list of driver routes.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt, Roles.Driver)]
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<List<ListDriverRoute>>> GetDriverRoutesByUser(Guid userId, FilterQuery? filter)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid (user ID)
                if (Guid.TryParse(subClaim, out var loggedUser) && roleClaim != null)
                {
                    var driverRoutes = await _driverRouteServices.GetDriverRoutesByUserAsync(userId, filter, roleClaim, loggedUser);
                    Response.Headers["X-Count"] = driverRoutes.Count.ToString(); // Set total count in response header
                    return Ok(driverRoutes); // Return list of driver routes
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." }); // Return 404 if 'sub' is not a valid Guid
                }

            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message }); // Return 404 for unexpected exceptions
            }
        }

        /// <summary>
        /// Retrieves a list of driver routes associated with a specific route.
        /// Accessible by Admin, AdminEnt.
        /// </summary>
        /// <param name="routeId">The ID of the user whose driver routes are to be retrieved.</param>
        /// <param name="filter">Optional filter query parameters for pagination or other criteria.</param>
        /// <returns>Returns a list of driver routes.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpGet("by-route/{routeId}")]
        public async Task<ActionResult<List<ListDriverRoute>>> GetDriverRoutesByRoute(Guid routeId, FilterQuery? filter)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid (user ID)
                if (Guid.TryParse(subClaim, out var loggedUser) && roleClaim != null)
                {
                    var driverRoutes = await _driverRouteServices.GetDriverRoutesByRouteIdAsync(routeId, loggedUser, roleClaim, filter);
                    Response.Headers["X-Count"] = driverRoutes.Count.ToString(); // Set total count in response header
                    return Ok(driverRoutes); // Return list of driver routes
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." }); // Return 404 if 'sub' is not a valid Guid
                }

            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message }); // Return 404 for unexpected exceptions
            }
        }

        /// <summary>
        /// Retrieves details of a specific driver route by ID.
        /// Accessible by Admin, AdminEnt, and Driver roles.
        /// </summary>
        /// <param name="driverRouteId">The ID of the driver route to retrieve.</param>
        /// <returns>Returns the details of the driver route.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt, Roles.Driver)]
        [HttpGet("{driverRouteId}")]
        public async Task<IActionResult> GetDriverRouteById(Guid driverRouteId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid (user ID)
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var driverRoute = await _driverRouteServices.GetDriverRouteByIdAsync(driverRouteId, userId, roleClaim);
                    return Ok(driverRoute); // Return driver route details
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." }); // Return 404 if 'sub' is not a valid Guid
                }

            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message }); // Return 404 for unexpected exceptions
            }
        }

        /// <summary>
        /// Deletes a driver-route association by ID.
        /// Accessible by Admin or AdminEnt roles.
        /// </summary>
        /// <param name="driverRouteId">The ID of the driver route to delete.</param>
        /// <returns>Returns a success message on successful deletion.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{driverRouteId}")]
        public async Task<IActionResult> DeleteDriverRoute(Guid driverRouteId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid (user ID)
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _driverRouteServices.DeleteDriverRouteAsync(driverRouteId, userId, roleClaim);
                    return Ok(new { message = "Driver-Route association deleted successfully" }); // Return success message
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." }); // Return 404 if 'sub' is not a valid Guid
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message }); // Return 500 for server errors
            }
        }
    }
}
