using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Models.Auth;
using PROG7311_GLMS_API.Services;

namespace PROG7311_GLMS_API.Controllers
{
    // Identity (users, passwords, roles) now lives entirely inside the API. The MVC frontend
    // never touches the database - it logs in here and receives a JWT.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IClientService _clientService;

        public AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService, IClientService clientService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _clientService = clientService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expires) = _tokenService.CreateToken(user, roles);

            return Ok(new LoginResponse
            {
                Token = token,
                Expires = expires,
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        // Creates the Identity login AND the Client profile in one transaction-like call,
        // so the frontend does not need its own UserManager.
        [HttpPost("register-client")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Client>> RegisterClient(RegisterClientRequest request)
        {
            var user = new IdentityUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, "Client");

            var client = await _clientService.CreateAsync(new Client
            {
                Name = request.Name,
                ContactDetails = request.ContactDetails,
                Region = request.Region,
                ApplicationUserId = user.Id
            });

            return Created($"/api/clients/{client.Id}", client);
        }
    }
}
