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
    /// Controller for managing audit-related data. Provides an endpoint to retrieve all audits with filtering options.
    /// </summary>
    [Route("api/v1/audit")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IAuditServices _auditServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditController"/> class.
        /// </summary>
        /// <param name="auditServices">The service responsible for audit-related operations.</param>
        public AuditController(IAuditServices auditServices)
        {
            _auditServices = auditServices;
        }

        /// <summary>
        /// Gets all audit records with optional filters. Only accessible by users with the Admin role.
        /// </summary>
        /// <param name="filter">Optional query parameters for filtering the audit records.</param>
        /// <returns>A list of audit records matching the filter criteria.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)] // Only accessible by users with Admin role
        [HttpGet]
        public async Task<ActionResult<Audit>> GetAllAudits(FilterQuery filter)
        {
            try
            {
                // Retrieve audits from the service with the provided filter
                var audits = await _auditServices.GetAuditsAsync(filter);

                // Set the total count in the response header for pagination or client-side processing
                Response.Headers["X-Count"] = audits.Count.ToString();

                // Return the list of audits with a 200 OK status
                return Ok(audits);
            }
            catch (Exception ex)
            {
                // Log the exception and return a 404 status code with the exception message
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
