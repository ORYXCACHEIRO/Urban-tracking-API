using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Requests.User;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing users, including creating, updating, retrieving, and deleting users.
    /// Accessible based on user roles such as Admin and AdminEnt (Admin Entity).
    /// </summary>
    [Route("api/v1/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        // Constructor takes IUserServices for user-related operations
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        /// <summary>
        /// Creates a new user.
        /// Accessible to users with Admin or AdminEnt roles.
        /// </summary>
        /// <param name="request">The data for the user to be created.</param>
        /// <returns>The created user or an error message.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<ActionResult<OneUser>> CreateUserAsync(PostUser request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
                            var user = await _userServices.CreateUserAsync(request);
                            return Ok(user);
                        case Roles.AdminEnt:
                            var driver = await _userServices.CreateDriverAsync(request, userId);
                            return Ok(driver);
                        default:
                            return StatusCode(404, new { message = "Error creating user" });
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
        /// Retrieves all users or drivers based on role and filter criteria.
        /// Accessible to users with Admin or AdminEnt roles.
        /// </summary>
        /// <param name="filter">Optional filter query parameters.</param>
        /// <returns>A list of users or drivers.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpGet]
        public async Task<ActionResult<ListUser>> GetAllUsers(FilterQuery? filter)
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
                            var users = await _userServices.GetUsersAsync(filter);
                            Response.Headers["X-Count"] = users.Count.ToString();
                            return Ok(users);
                        case Roles.AdminEnt:
                            var drivers = await _userServices.GetDriversAsync(userId, filter);
                            Response.Headers["X-Count"] = drivers.Count.ToString();
                            return Ok(drivers);
                        default:
                            return StatusCode(404, new { message = "Error getting users" });
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
        /// Retrieves a specific user or driver by their ID.
        /// Accessible to users with Admin or AdminEnt roles.
        /// </summary>
        /// <param name="id">The ID of the user or driver.</param>
        /// <returns>The user or driver with the specified ID.</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OneUser>> GetUser(Guid id)
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
                        case Roles.AdminEnt:
                            var driver = await _userServices.GetDriverByIdAsync(id, userId);
                            return Ok(driver);
                        default:
                            var user = await _userServices.GetByIdAsync(id, userId, roleClaim);
                            return Ok(user);
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
        /// Retrieves users who are not associated with an entity.
        /// Accessible to users with Admin or AdminEnt roles.
        /// </summary>
        /// <param name="filter">Optional filter query parameters.</param>
        /// <returns>A list of users without entity associations.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpGet("no-entity")]
        public async Task<IActionResult> GetUsersWithoutEntity(FilterQuery? filter)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");
                 
                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var usersWithouEnt = await _userServices.GetUnassociatedUsersAsync(userId, roleClaim, filter);
                    Response.Headers["X-Count"] = usersWithouEnt.Count.ToString();
                    return Ok(usersWithouEnt);
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
        /// Updates an existing user or driver.
        /// Accessible to users with Admin or AdminEnt roles.
        /// </summary>
        /// <param name="request">The data to update the user.</param>
        /// <param name="id">The ID of the user to update.</param>
        /// <returns>A message indicating whether the user was successfully updated.</returns>
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUserAsync(PatchUser request, Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    switch (roleClaim)
                    {
                        case Roles.AdminEnt:
                            await _userServices.UpdateDriverAsync(id, userId, request);
                            return Ok(new { message = "User successfully edited" });
                        default:
                            await _userServices.UpdateUserAsync(id, userId, roleClaim, request);
                            return Ok(new { message = "User successfully edited" });
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
        /// Deletes a user or driver by their ID.
        /// Accessible to users with Admin or AdminEnt roles.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A message indicating whether the user was successfully deleted.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
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
                        case Roles.AdminEnt:
                            await _userServices.DeleteDriverAsync(id, userId);
                            return Ok(new { message = "User successfully deleted" });
                        default:
                            await _userServices.DeleteUserAsync(id, userId);
                            return Ok(new { message = "User successfully deleted" });
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
    }
}
