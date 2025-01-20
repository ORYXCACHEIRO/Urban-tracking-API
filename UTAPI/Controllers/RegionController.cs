using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UTAPI.Interfaces;
using UTAPI.Models;
using UTAPI.Requests.Region;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing regions.
    /// Supports creating, retrieving, updating, and deleting regions.
    /// Content negotiation allows the response to be in either JSON or XML format based on the `Accept` header.
    /// </summary>
    [Route("api/v1/region")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly IRegionServices _regionServices;
        private readonly ILogger<RegionController> _logger;

        public RegionController(IRegionServices regionServices, ILogger<RegionController> logger)
        {
            _regionServices = regionServices;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new region.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="request">The region details to create.</param>
        /// <returns>The created region details.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpPost]
        public async Task<ActionResult<OneRegion>> CreateRegionAsync(PostRegion request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var region = await _regionServices.CreateRegionAsync(request);
                return Ok(region); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating region");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of all regions.
        /// Supports content negotiation to return results in JSON or XML format depending on the client's request.
        /// </summary>
        /// <param name="filter">Optional filter parameters for retrieving the regions.</param>
        /// <returns>A list of all regions.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ListRegion>> GetAllRegions(FilterQuery? filter)
        {
            try
            {
                var regions = await _regionServices.GetRegionsAsync(filter);

                // Add the count of the regions to the response headers
                Response.Headers["X-Count"] = regions.Count.ToString();

                return Ok(regions); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all regions");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a region by its ID.
        /// The response format (JSON or XML) is determined by the client's `Accept` header.
        /// </summary>
        /// <param name="id">The ID of the region to retrieve.</param>
        /// <returns>The details of the requested region.</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OneRegion>> GetRegion(Guid id)
        {
            try
            {
                var region = await _regionServices.GetByIdAsync(id);
                return Ok(region); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving region with ID {id}");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing region by its ID.
        /// Supports content negotiation to return results in JSON or XML format.
        /// </summary>
        /// <param name="request">The updated region details.</param>
        /// <param name="id">The ID of the region to update.</param>
        /// <returns>Success message if the region was updated.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRegionAsync(PatchRegion request, Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _regionServices.UpdateRegionAsync(id, request);
                return Ok(new { message = "Region successfully edited" }); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating region with ID {id}");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a region by its ID.
        /// Supports content negotiation to return results in JSON or XML format.
        /// </summary>
        /// <param name="id">The ID of the region to delete.</param>
        /// <returns>Success message if the region was deleted.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRegionAsync(Guid id)
        {
            try
            {
                await _regionServices.DeleteRegionAsync(id);
                return Ok(new { message = "Region successfully deleted" }); // Response formatted based on `Accept` header
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting region with ID {id}");
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }
    }
}
