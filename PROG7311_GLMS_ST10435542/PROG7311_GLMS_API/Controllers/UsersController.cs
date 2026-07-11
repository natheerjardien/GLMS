using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_API.Models.Auth;

namespace PROG7311_GLMS_API.Controllers
{
    // User lookups the frontend needs (e.g. the driver dropdown) without giving it database access.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager) => _userManager = userManager;

        [HttpGet("drivers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetDrivers()
        {
            var drivers = await _userManager.GetUsersInRoleAsync("Driver");
            return Ok(drivers.Select(d => new UserDto { Id = d.Id, UserName = d.UserName ?? string.Empty }));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);
            return NoContent();
        }
    }
}
