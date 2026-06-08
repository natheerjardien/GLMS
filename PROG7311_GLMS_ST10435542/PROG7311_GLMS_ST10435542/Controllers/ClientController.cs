using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_ST10435542.Data;
using PROG7311_GLMS_ST10435542.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff")] // both admin and staff can access this controller
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ClientController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class ClientCreateViewModel
        {
            public int Id { get; set; }
            [Required] 
            public string Name { get; set; }
            [Required] 
            public string ContactDetails { get; set; }
            [Required] 
            public string Region { get; set; }
            [Required, EmailAddress] 
            public string Email { get; set; }
            [Required, DataType(DataType.Password)] 
            public string Password { get; set; }
        }

        // GET: Client
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.ToListAsync());
        }

        // GET: Client/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Client/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Client/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

                    _context.Clients.Add(client);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Client and Login account created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Client/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var model = new ClientCreateViewModel
            {
                Id = client.Id, // Ensure Id is in your ViewModel!
                Name = client.Name,
                ContactDetails = client.ContactDetails,
                Region = client.Region
            };

            return View(model);
        }

        // POST: Client/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                try
                {
                    var client = await _context.Clients.FindAsync(id);
                    
                    if (client == null)
                    {
                        return NotFound();
                    }

                    client.Name = model.Name;
                    client.ContactDetails = model.ContactDetails;
                    client.Region = model.Region;

                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Client/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Client/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(m => m.Id == id);

                if (client == null)
                {
                    TempData["Error"] = "Client not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (client.Contracts.Any())
                {
                    TempData["Error"] = "Oh snap! You cannot delete a client that has active contracts. Delete the contracts first.";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.FindByIdAsync(client.ApplicationUserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Client and associated Account successfully removed from the system.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "A database error occurred while trying to delete the client.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
