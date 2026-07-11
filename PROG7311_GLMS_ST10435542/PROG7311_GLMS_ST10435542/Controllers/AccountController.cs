using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PROG7311_GLMS_ST10435542.Models.Auth;
using PROG7311_GLMS_ST10435542.Services.ApiClients;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    // Replaces the old ASP.NET Identity area. The frontend no longer owns any user data:
    // it sends the credentials to the API, receives a JWT + roles back, and stores them
    // inside an ordinary auth cookie for the browser session.
    public class AccountController : Controller
    {
        private readonly AuthApiClient _authApiClient;

        public AccountController(AuthApiClient authApiClient) => _authApiClient = authApiClient;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authApiClient.LoginAsync(model.Email, model.Password);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.UserId),
                    new(ClaimTypes.Name, result.Email),
                    new("access_token", result.Token) // the JWT rides inside the cookie for API calls
                };
                claims.AddRange(result.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties { ExpiresUtc = result.Expires });

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                // The API container/service is unreachable - fail gracefully instead of crashing
                ModelState.AddModelError(string.Empty, "Could not reach the GLMS API. Please try again shortly.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
