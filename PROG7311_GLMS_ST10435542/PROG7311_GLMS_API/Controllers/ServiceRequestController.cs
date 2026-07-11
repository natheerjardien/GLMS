using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Services;

namespace PROG7311_GLMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestsController(IServiceRequestService serviceRequestService) =>
            _serviceRequestService = serviceRequestService;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetAll() =>
            Ok(await _serviceRequestService.GetAllAsync());

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceRequest>> GetById(int id)
        {
            var sr = await _serviceRequestService.GetByIdAsync(id);
            return sr == null ? NotFound() : Ok(sr);
        }

        // The business rule (only ACTIVE contracts can receive requests) and the currency
        // calculation both run inside the service layer, so the API is the single gatekeeper.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ServiceRequest>> Create(ServiceRequest serviceRequest)
        {
            try
            {
                var created = await _serviceRequestService.CreateAsync(serviceRequest);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return BadRequest("The id in the URL does not match the id in the body.");
            }

            var updated = await _serviceRequestService.UpdateAsync(id, serviceRequest);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _serviceRequestService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
