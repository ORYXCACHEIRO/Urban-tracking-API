using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.PriceTable;
using UTAPI.Security;
using UTAPI.Services;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing Price Tables.
    /// Supports creating, retrieving, updating, and deleting price tables.
    /// Content negotiation allows the response to be in either JSON or XML format based on the `Accept` header.
    /// </summary>
    [Route("api/v1/price-table")]
    [ApiController]
    public class PriceTableController : ControllerBase
    {
        private readonly IPriceTableServices _priceTableServices;
        private readonly ILogger<EntityServices> _logger; // facilitates logging

        public PriceTableController(IPriceTableServices priceTableServices, ILogger<EntityServices> logger)
        {
            _priceTableServices = priceTableServices;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new price table.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="request">The price table details to create.</param>
        /// <returns>The created price table details.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<IActionResult> CreateIPriceTableServicesAsync(PostPriceTable request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var priceTable = await _priceTableServices.CreatePriceTableAsync(request, userId, roleClaim);
                    return Ok(priceTable); // Response formatted based on `Accept` header
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
        /// Retrieves a price table by its entity ID.
        /// The client can specify `Accept: application/xml` to receive the response in XML format.
        /// </summary>
        /// <param name="entityId">The entity ID associated with the price table.</param>
        /// <param name="filter">Optional filters for the price table.</param>
        /// <returns>The price table associated with the entity ID.</returns>
        [Authorize]
        [HttpGet("{entityId}")]
        public async Task<ActionResult<List<ListPriceTable>>> GetPriceTableByEntIdAsync(Guid entityId, FilterQuery? filter)
        {
            try
            {
                var roleClaim = User.FindFirstValue("role");

                if (roleClaim != null)
                {
                    // Call the service method to get the price table by entity ID
                    var result = await _priceTableServices.GetPriceTableByEntityIdAsync(entityId, filter, roleClaim);

                    // Return the result with a 200 OK status
                    return Ok(result); // Response formatted based on `Accept` header
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a 500 Internal Server Error status
                _logger.LogError(ex, "An error occurred while retrieving the price table");
                return StatusCode(404, new { message = "An error occurred while retrieving the price table", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a price table by its ID.
        /// Supports content negotiation for JSON or XML based on the client's `Accept` header.
        /// </summary>
        /// <param name="id">The ID of the price table to delete.</param>
        /// <returns>Success message.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePriceTableAsync(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _priceTableServices.DeletePriceTableAsync(id, userId, roleClaim);
                    return Ok(new { message = "Price Table Successfully Deleted" }); // Response formatted based on `Accept` header
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
        /// Updates an existing price table by its ID.
        /// Supports XML responses if requested by the client.
        /// </summary>
        /// <param name="id">The ID of the price table to update.</param>
        /// <param name="request">The updated price table details.</param>
        /// <returns>Success message.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePriceTableAsync(Guid id, PatchPriceTable request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _priceTableServices.UpdatePriceTableAsync(id, request, userId, roleClaim);
                    return Ok(new { message = "Price Table successfully edited" }); // Response formatted based on `Accept` header
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
