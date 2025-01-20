using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Requests.PriceTable;
using UTAPI.Security;
using UTAPI.Services;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing price table content. Provides operations for creating, retrieving, updating, and deleting price table content.
    /// Accessible only by users with "Admin" or "AdminEnt" roles.
    /// </summary>
    [Route("api/v1/price-table-content/")]
    [ApiController]
    public class PriceTableContentController : ControllerBase
    {
        private readonly IPriceTableContentServices _priceTableContentServices;
        private readonly ILogger<EntityServices> _logger; // Facilitates logging for troubleshooting

        /// <summary>
        /// Initializes a new instance of the PriceTableContentController.
        /// </summary>
        /// <param name="priceTableContentServices">The service responsible for price table content operations.</param>
        /// <param name="logger">Logger for tracking errors and events.</param>
        public PriceTableContentController(IPriceTableContentServices priceTableContentServices, ILogger<EntityServices> logger)
        {
            _priceTableContentServices = priceTableContentServices;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new price table content entry. Accessible only by users with the "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="request">The request object containing the price table content details.</param>
        /// <returns>An IActionResult indicating the result of the creation operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPost]
        public async Task<IActionResult> CreateIPriceTableContentServicesAsync(PostPriceTableContent request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var priceTableContent = await _priceTableContentServices.CreatePriceTableContentAsync(request, userId, roleClaim);
                    return Ok(priceTableContent);
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }

            }
            catch (Exception ex)
            {
                // Log the exception for troubleshooting
                _logger.LogError(ex, "Error occurred while creating price table content");
                return StatusCode(500, new { message = ex.Message }); // Changed to 500 for server errors
            }
        }

        /// <summary>
        /// Retrieves price table content by price table ID. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="priceTableId">The ID of the price table for which content is being retrieved.</param>
        /// <returns>An IActionResult containing the price table content.</returns>
        [Authorize]
        [HttpGet("{priceTableId}")]
        public async Task<IActionResult> GetPriceTableContentByPriceTableId(Guid priceTableId)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    var priceTableContent = await _priceTableContentServices.GetPriceTableContentByPriceTableIdAsync(priceTableId, userId, roleClaim);
                    return Ok(priceTableContent);
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }
            }
            catch (Exception ex)
            {
                // Log and return an error response if something goes wrong
                _logger.LogError(ex, "An error occurred while retrieving price table content");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a price table content entry by its ID. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="id">The ID of the price table content to delete.</param>
        /// <returns>An IActionResult indicating the result of the deletion operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePriceTableContentAsync(Guid id)
        {
            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _priceTableContentServices.DeletePriceTableContentAsync(id, userId, roleClaim);
                    return Ok(new { message = "Price Table Successfully Deleted" });
                }
                else
                {
                    return StatusCode(404, new { message = "Invalid user ID format." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting price table content");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates a price table content entry. Accessible only by users with "Admin" or "AdminEnt" roles.
        /// </summary>
        /// <param name="id">The ID of the price table content to update.</param>
        /// <param name="request">The request object containing updated price table content details.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin, Roles.AdminEnt)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEntityAsync(Guid id, PatchPriceTableContent request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var subClaim = User.FindFirstValue("sub");
                var roleClaim = User.FindFirstValue("role");

                // Check if 'sub' is a valid Guid
                if (Guid.TryParse(subClaim, out var userId) && roleClaim != null)
                {
                    await _priceTableContentServices.UpdatePriceTableContentAsync(id, request, userId, roleClaim);
                    return Ok(new { message = "Price Table successfully edited" });
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
