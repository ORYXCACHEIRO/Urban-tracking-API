using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Requests.FavRoute;
using UTAPI.Security;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing favorite routes. Provides operations for creating, retrieving, and deleting favorite routes.
    /// Accessible only by users with the "User" role.
    /// </summary>
    [Route("api/v1/mobile/fav-route")]
    [ApiController]
    public class FavRouteController : ControllerBase
    {
        private readonly IFavRouteServices _favRouteServices;

        /// <summary>
        /// Initializes a new instance of the FavRouteController with the provided favorite route services.
        /// </summary>
        /// <param name="favRouteServices">The service responsible for favorite route operations.</param>
        public FavRouteController(IFavRouteServices favRouteServices)
        {
            _favRouteServices = favRouteServices;
        }

        /// <summary>
        /// Creates a new favorite route for a user. Accessible only by users with the "User" role.
        /// </summary>
        /// <param name="request">The request object containing the favorite route details.</param>
        /// <returns>An IActionResult indicating the result of the creation operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.User)]
        [HttpPost]
        public async Task<IActionResult> CreateFavRouteAsync(PostFavRoute request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId))
                {
                    var favRoute = await _favRouteServices.CreateFavRouteAsync(request, userId);
                    return Ok(favRoute);
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
        /// Retrieves a favorite route for a user by the route ID. Accessible only by users with the "User" role.
        /// </summary>
        /// <param name="id">The ID of the favorite route to retrieve.</param>
        /// <returns>An IActionResult containing the favorite route.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.User)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFavRouteByUserId(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId))
                {
                    var routes = await _favRouteServices.GetFavRouteByUserIdAsync(id, userId);
                    return Ok(routes);
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
        /// Deletes a favorite route for a user by its ID. Accessible only by users with the "User" role.
        /// </summary>
        /// <param name="id">The ID of the favorite route to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.User)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFavRouteAsync(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId))
                {
                    await _favRouteServices.DeleteFavRouteAsync(id, userId);
                    return Ok(new { message = "Fav Route Successfully Deleted" });
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
