using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing user sessions, including retrieving session data and deactivating sessions.
    /// Accessible only to users with Admin roles.
    /// </summary>
    [Route("api/v1/session")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ISessionServices _sessionServices;

        // Constructor takes ISessionServices instead of concrete session service
        public SessionController(ISessionServices sessionServices)
        {
            _sessionServices = sessionServices;
        }

        /// <summary>
        /// Retrieves all session audits for a specific user.
        /// Accessible only to users with Admin roles.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="userId">The ID of the user whose sessions are being retrieved.</param>
        /// <param name="filter">Optional query parameters for filtering the session results.</param>
        /// <returns>A list of sessions for the specified user based on the provided filter query.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<Session>> GetAllAudits(Guid userId, FilterQuery filter)
        {
            try
            {
                // Retrieve sessions for the user based on the filter query
                var sessions = await _sessionServices.GetSessionsByUserAsync(filter, userId);
                Response.Headers["X-Count"] = sessions.Count.ToString(); // Include total count in response headers
                return Ok(sessions); // Return session data, formatted based on `Accept` header (JSON or XML)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivates a specific session by its ID.
        /// Accessible only to users with Admin roles.
        /// </summary>
        /// <param name="id">The ID of the session to deactivate.</param>
        /// <returns>A message indicating whether the session was successfully deactivated.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeactivateSession(Guid id)
        {
            try
            {
                // Deactivate the session by its ID
                await _sessionServices.DeactivateSessionAsync(id);
                return Ok(new { message = "Session deactivated" }); // Return success message
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
