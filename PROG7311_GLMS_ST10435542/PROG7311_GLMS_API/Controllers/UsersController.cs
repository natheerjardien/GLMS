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
        // Used UserManager to retrieve users belonging to specific roles for assignment lookups (Microsoft, 2026b).
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
/* Reference List:
    Microsoft. 2026a. Create web APIs with ASP.NET Core. [online] Available at: https://learn.microsoft.com/en-us/aspnet/core/web-api/ [Accessed 22 April 2026].
    Microsoft. 2026b. Introduction to Identity on ASP.NET Core. [online] Available at: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity [Accessed 22 April 2026].
*/