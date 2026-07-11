using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

using PROG7311_GLMS_ST10435542.Models;
using PROG7311_GLMS_ST10435542.Services.ApiClients;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff")] 
    public class ClientController : Controller
    {
        private readonly ClientApiClient _apiClient;
        private readonly UserManager<IdentityUser> _userManager;

        public ClientController(ClientApiClient apiClient, UserManager<IdentityUser> userManager)
        {
            _apiClient = apiClient;
            _userManager = userManager;
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
            return View(await _apiClient.GetAllAsync());
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
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");

                    var client = new Client
                    {
                        Name = model.Name,
                        ContactDetails = model.ContactDetails,
                        Region = model.Region,
                        ApplicationUserId = user.Id
                    };

                    await _apiClient.CreateAsync(client);
                    
                    TempData["Success"] = "Client and Login account created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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

                await _apiClient.UpdateAsync(id, client);
                return RedirectToAction(nameof(Index));
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

                var user = await _userManager.FindByIdAsync(client.ApplicationUserId);
                
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                TempData["Success"] = "Client and associated Account successfully removed.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error deleting client.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}