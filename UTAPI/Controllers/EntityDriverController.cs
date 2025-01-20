using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.EntityDriver;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing entity drivers in the system. Provides CRUD operations for entity drivers,
    /// with role-based access control to ensure only authorized users (Admin, AdminEnt) can perform certain actions.
    /// </summary>
    [Route("api/v1/entity/driver")]
    [ApiController]
    public class EntityDriverController : ControllerBase
    {
        private readonly IEntityDriverServices _entityDriverServices;

        /// <summary>
        /// Initializes a new instance of the EntityDriverController with the provided entity driver services.
        /// </summary>
        /// <param name="favRouteServices">The service responsible for entity driver-related operations.</param>
        public EntityDriverController(IEntityDriverServices favRouteServices)
        {
            _entityDriverServices = favRouteServices;
        }

        /// <summary>
        /// Creates a new entity driver. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="request">The request object containing the entity driver details.</param>
        /// <returns>An IActionResult indicating the result of the creation operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<IActionResult> CreateEntityDriverAsync(PostEntityDriver request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var entityDriver = await _entityDriverServices.CreateEntityDriverAsync(request, userId, roleClaim);
                    return Ok(entityDriver);
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
        /// Retrieves an entity driver by user ID. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="id">The ID of the entity driver to retrieve.</param>
        /// <returns>The entity driver associated with the provided user ID.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> GetEntityDrivers(FilterQuery? filter)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var route = await _entityDriverServices.GetEntityDriversAsync(userId, roleClaim, filter);
                    return Ok(route);
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
        /// Retrieves an entity driver by user ID. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="userId">The ID of the entity driver to retrieve.</param>
        /// <returns>The entity driver associated with the provided user ID.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetEntityDriverByUserId(Guid userId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var loggedId) && roleClaim != null)
                {
                    var route = await _entityDriverServices.GetEntityDriverByUserIdAsync(userId, loggedId, roleClaim);
                    return Ok(route);
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
        /// Deletes an entity driver by its unique ID. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="id">The ID of the entity driver to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEntityDriverAsync(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _entityDriverServices.DeleteEntityDriverAsync(id, userId, roleClaim);
                    return Ok(new { message = "Entity Driver Removed" });
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
        /// Retrieves all entity drivers associated with a specific entity by entity ID. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="entityId">The ID of the entity whose associated drivers are being retrieved.</param>
        /// <returns>A list of entity drivers associated with the provided entity ID.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpGet("entity/{entityId:guid}")]
        public async Task<IActionResult> GetEntityDriversByEntityId(Guid entityId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var entityDrivers = await _entityDriverServices.GetEntityDriversByEntityIdAsync(entityId, userId, roleClaim);
                    Response.Headers["X-Count"] = entityDrivers.Count.ToString();
                    return Ok(entityDrivers);
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
