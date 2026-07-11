using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

using PROG7311_GLMS_ST10435542.Models;
using PROG7311_GLMS_ST10435542.Models.Auth;
using PROG7311_GLMS_ST10435542.Services.ApiClients;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ClientController : Controller
    {
        private readonly ClientApiClient _apiClient;
        private readonly AuthApiClient _authApiClient;
        private readonly UsersApiClient _usersApiClient;

        public ClientController(ClientApiClient apiClient, AuthApiClient authApiClient, UsersApiClient usersApiClient)
        {
            _apiClient = apiClient;
            _authApiClient = authApiClient;
            _usersApiClient = usersApiClient;
        }

        public class ClientCreateViewModel
        {
            public int Id { get; set; }

            [Required]
            public string Name { get; set; } = string.Empty;

            [Required]
            public string ContactDetails { get; set; } = string.Empty;

            [Required]
            public string Region { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _apiClient.GetAllAsync());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not reach the GLMS API.";
                return View(Enumerable.Empty<Client>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // One API call creates the Identity login AND the client profile -
                    // the frontend no longer has a UserManager of its own.
                    await _authApiClient.RegisterClientAsync(new RegisterClientRequest
                    {
                        Email = model.Email,
                        Password = model.Password,
                        Name = model.Name,
                        ContactDetails = model.ContactDetails,
                        Region = model.Region
                    });

                    TempData["Success"] = "Client and Login account created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Could not reach the GLMS API.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            var model = new ClientCreateViewModel
            {
                Id = client.Id,
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientCreateViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Email/Password are only used when creating a brand-new client login
            ModelState.Remove("Email");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                var client = await _apiClient.GetByIdAsync(id);

                if (client == null)
                {
                    return NotFound();
                }

                client.Name = model.Name;
                client.ContactDetails = model.ContactDetails;
                client.Region = model.Region;

                try
                {
                    await _apiClient.UpdateAsync(id, client);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var client = await _apiClient.GetByIdAsync(id);

                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _apiClient.DeleteAsync(id);

                // Remove the linked login account through the API as well
                if (!string.IsNullOrEmpty(client.ApplicationUserId))
                {
                    await _usersApiClient.DeleteUserAsync(client.ApplicationUserId);
                }

                TempData["Success"] = "Client and associated Account successfully removed.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "Cannot delete. " + ex.Message;
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not reach the GLMS API.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
