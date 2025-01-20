using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UTAPI.Interfaces;
using UTAPI.Requests.Stop;
using UTAPI.Security;
using UTAPI.Types;
using UTAPI.Utils;

namespace UTAPI.Controllers
{
    /// <summary>
    /// Controller for managing stops, including creating, updating, retrieving, and deleting stops.
    /// Accessible only to users with Admin roles for certain actions.
    /// </summary>
    [Route("api/v1/stop")]
    [ApiController]
    public class StopController : ControllerBase
    {
        private readonly IStopServices _stopServices;

        // Constructor takes IStopServices instead of concrete stop service
        public StopController(IStopServices stopServices)
        {
            _stopServices = stopServices;
        }

        /// <summary>
        /// Creates a new stop.
        /// Accessible only to users with Admin roles.
        /// </summary>
        /// <param name="request">The data for the new stop to be created.</param>
        /// <returns>The created stop.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateStopAsync(PostStop request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Call service to create the new stop
                var stop = await _stopServices.CreateStopAsync(request);
                return Ok(stop); // Return the created stop with a 200 OK status
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all stops with optional filtering.
        /// Accessible by any authenticated user.
        /// </summary>
        /// <param name="filter">Optional query parameters for filtering the stops.</param>
        /// <returns>A list of all stops matching the filter.</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<ListStop>>> GetAllStops(FilterQuery filter)
        {
            try
            {
                // Retrieve list of stops based on the filter query
                var stops = await _stopServices.GetStopsAsync(filter);
                Response.Headers["X-Count"] = stops.Count.ToString(); // Include total count in response headers
                return Ok(stops); // Return the list of stops
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific stop by its ID.
        /// Accessible by any authenticated user.
        /// </summary>
        /// <param name="id">The ID of the stop to retrieve.</param>
        /// <returns>The stop with the specified ID.</returns>
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OneStop>> GetStop(Guid id)
        {
            try
            {
                // Retrieve stop by ID
                var stop = await _stopServices.GetByIdAsync(id);
                return Ok(stop); // Return the stop
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing stop.
        /// Accessible only to users with Admin roles.
        /// </summary>
        /// <param name="request">The data to update the stop.</param>
        /// <param name="id">The ID of the stop to update.</param>
        /// <returns>A message indicating whether the stop was successfully updated.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateStopAsync(PatchStop request, Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Call service to update the stop
                await _stopServices.UpdateStopAsync(id, request);
                return Ok(new { message = "Stop successfully updated" }); // Return success message
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a stop by its ID.
        /// Accessible only to users with Admin roles.
        /// </summary>
        /// <param name="id">The ID of the stop to delete.</param>
        /// <returns>A message indicating whether the stop was successfully deleted.</returns>
        [Authorize]
        [RequiresClaim(IdentityData.UserRoleClaimName, Roles.Admin)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteStopAsync(Guid id)
        {
            try
            {
                // Call service to delete the stop by ID
                await _stopServices.DeleteStopAsync(id);
                return Ok(new { message = "Stop successfully deleted" }); // Return success message
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
