using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Services;

namespace PROG7311_GLMS_API.Controllers
{
    // Thin REST controller: no database or business logic here, everything is delegated to the
    // service layer (which in turn uses repositories). This is the Separation of Concerns the
    // SOA refactor is about.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService) => _contractService = contractService;

        // GET /api/contracts?status=Active&startDate=2026-01-01&endDate=2026-12-31
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Contract>>> GetAll(
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var contracts = await _contractService.GetContractsAsync(status, startDate, endDate);
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Contract>> GetById(int id)
        {
            var contract = await _contractService.GetByIdAsync(id);
            return contract == null ? NotFound() : Ok(contract);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Contract>> Create(Contract contract)
        {
            try
            {
                var newContract = await _contractService.CreateAsync(contract);
                return CreatedAtAction(nameof(GetById), new { id = newContract.Id }, newContract);
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
        public async Task<IActionResult> Update(int id, Contract contract)
        {
            if (id != contract.Id)
            {
                return BadRequest("The id in the URL does not match the id in the body.");
            }

            try
            {
                var updated = await _contractService.UpdateAsync(id, contract);
                return updated ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PATCH /api/contracts/5/status  (body: "Activate" | "Expire" | "Hold")
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string action)
        {
            try
            {
                var contract = await _contractService.ChangeStatusAsync(id, action);
                return contract == null ? NotFound() : Ok(contract);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _contractService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
