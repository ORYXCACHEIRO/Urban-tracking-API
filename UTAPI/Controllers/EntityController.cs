using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Requests.Entity;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing entities in the system. Provides CRUD operations for entities,
    /// with role-based access control to ensure only authorized users can perform certain actions.
    /// </summary>
    [Route("api/v1/entity")]
    [ApiController]
    public class EntityController : ControllerBase
    {
        private readonly IEntityServices _entityServices;

        /// <summary>
        /// Initializes a new instance of the EntityController with the provided entity services.
        /// </summary>
        /// <param name="entityServices">The service responsible for entity-related operations.</param>
        public EntityController(IEntityServices entityServices)
        {
            _entityServices = entityServices;
        }

        /// <summary>
        /// Creates a new entity. Only accessible by users with the "Admin" role.
        /// </summary>
        /// <param name="request">The request object containing the entity details.</param>
        /// <returns>An IActionResult indicating the result of the creation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateEntityAsync(PostEntity request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var entity = await _entityServices.CreateEntityAsync(request);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of all entities with optional filtering. Accessible to all authorized users.
        /// </summary>
        /// <param name="filter">The filter query to apply to the results.</param>
        /// <returns>A list of entities matching the filter criteria.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ListEntity>> GetAllEntities(FilterQuery filter)
        {
            try
            {
                var entities = await _entityServices.GetEntitiesAsync(filter);

                Response.Headers["X-Count"] = entities.Count.ToString();

                return Ok(entities);
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single entity by its unique ID. Accessible to all authorized users.
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve.</param>
        /// <returns>The entity with the specified ID.</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OneEntity>> GetEntity(Guid id)
        {
            try
            {
                var entity = await _entityServices.GetByIdAsync(id);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the entity associated with the provided user ID. Accessible to users with the "AdminEnt" or "Driver" roles.
        /// </summary>
        /// <param name="userId">The user ID to find the associated entity.</param>
        /// <returns>The entity associated with the user.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.AdminEnt, Roles.Driver)]
        [HttpGet("my-entity/{userId:guid}")]
        public async Task<ActionResult<OneEntity>> GetEntityOfUser(Guid userId)
        {
            try
            {
                var entity = await _entityServices.GetByUserIdAsync(userId);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing entity. Accessible only to users with the "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="request">The updated entity data.</param>
        /// <param name="id">The ID of the entity to update.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEntityAsync(PatchEntity request, Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _entityServices.UpdateEntityAsync(id, request, userId, roleClaim);
                    return Ok(new { message = "Entity successfully edited" });
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
        /// Deletes an entity by its unique ID. Only accessible by users with the "Admin" role.
        /// </summary>
        /// <param name="id">The ID of the entity to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEntityAsync(Guid id)
        {
            try
            {
                await _entityServices.DeleteEntityAsync(id);
                return Ok(new { message = "Entity successfully deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
