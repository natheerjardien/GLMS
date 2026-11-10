using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Services;

namespace PROG7311_GLMS_API.Controllers
{
    // Used Authorize attribute to restrict this entire controller to users with a valid JWT token (Microsoft, 2026b).
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService) => _clientService = clientService;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll() =>
            Ok(await _clientService.GetAllAsync());

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Client>> GetById(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            return client == null ? NotFound() : Ok(client);
        }

        // Used CreatedAtAction to return a 201 status code along with a Location header pointing to the new resource (Microsoft, 2026a).
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Client>> Create(Client client)
        {
            var created = await _clientService.CreateAsync(client);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Client client)
        {
            if (id != client.Id)
            {
                return BadRequest("The id in the URL does not match the id in the body.");
            }

            var updated = await _clientService.UpdateAsync(id, client);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            if (client.Contracts.Any())
            {
                return BadRequest("Cannot delete a client that still has contracts.");
            }

            await _clientService.DeleteAsync(id);
            return NoContent();
        }
    }
}
/* Reference List:
    Microsoft. 2026a. Create web APIs with ASP.NET Core. [online] Available at: https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 22 April 2026].
    Microsoft. 2026b. Introduction to Identity on ASP.NET Core. [online] Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity [Accessed 22 April 2026].
*/