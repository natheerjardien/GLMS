using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_ST10435542.Data;
using PROG7311_GLMS_ST10435542.Models;
using PROG7311_GLMS_ST10435542.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff,Client,Driver")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyConversionStrategy _currencyStrategy;
        private readonly IPricingService _pricingService;
        private readonly UserManager<IdentityUser> _userManager;

        public ServiceRequestController(ApplicationDbContext context, ICurrencyConversionStrategy currencyStrategy, IPricingService pricingService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _currencyStrategy = currencyStrategy;
            _pricingService = pricingService;
            _userManager = userManager;
        }

        // GET: ServiceRequest
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client);

            var list = await applicationDbContext.ToListAsync();

            var drivers = await _userManager.GetUsersInRoleAsync("Driver");
            ViewBag.DriverDict = drivers.ToDictionary(d => d.Id, d => d.UserName);

            return View(list);
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyRequests() // new view for the client only
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var myRequests = await _context.ServiceRequests
                .Include(s => s.Client)
                .Where(s => s.Client.ApplicationUserId == userId) 
                .ToListAsync();

            return View("MyRequests", myRequests);
        }

        [Authorize(Roles = "Admin,Staff,Driver")]
        public async Task<IActionResult> DriverIndex()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var assignedRequests = await _context.ServiceRequests
                .Where(s => s.AssignedDriverId == userId) 
                .ToListAsync();

            return View("DriverIndex", assignedRequests);
        }

        // GET: ServiceRequest/Details/5
        [Authorize(Roles = "Admin,Staff,Client,Driver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        [Authorize(Roles = "Admin,Staff,Client")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var contractQuery = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (User.IsInRole("Client"))
            {
                contractQuery = contractQuery.Where(c => c.Status == "Active" && c.Client.ApplicationUserId == userId);
            }
            else
            {
                contractQuery = contractQuery.Where(c => c.Status == "Active");
            }

            var contractList = contractQuery.Select(c => new {
                Id = c.Id,
                DisplayText = c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
            });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText");

            var drivers = _userManager.GetUsersInRoleAsync("Driver").Result;
            
            ViewBag.DriverList = new SelectList(drivers, "Id", "UserName");

            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff,Client")]
        public async Task<IActionResult> Create([Bind("Id,ContractId,Description,OriginalCost,PickupAddress,DeliveryAddress,RecipientName,RecipientPhone,SlaType,PackageSizeCategory,AssignedDriverId")] ServiceRequest serviceRequest)
        {
            ModelState.Remove("Contract"); // remove validation for Contract since we are only binding the ContractId and not the entire Contract object
            ModelState.Remove("Status"); // removes validation for Status since we will set it to "Pending" by default in the controller    
            ModelState.Remove("Cost");
            ModelState.Remove("RequestDate");
            ModelState.Remove("AssignedDriverId");
            ModelState.Remove("Client");

            var parentContract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == serviceRequest.ContractId);

            if (parentContract == null || parentContract.Status != "Active") // strict business rule to block if contract is missing, draft, expired, or on hold
            {
                ModelState.AddModelError("ContractId", "Cannot create a service request for an Expired or On Hold contract.");
            }
            else
            {
                serviceRequest.ClientId = parentContract.ClientId; 
            }

            if (User.IsInRole("Client"))
            {               
                serviceRequest.AssignedDriverId = null; // clients cannot assign drivers, so this ensures that the assigned driver is always null when a client creates a service request
            }

            if (ModelState.IsValid)
            {
                serviceRequest.Status = "Pending";
                serviceRequest.RequestDate = DateTime.Now;

                serviceRequest.Cost = await _pricingService.GetTotalCostInUsdAsync(serviceRequest.PackageSizeCategory, serviceRequest.SlaType); // calculates the final ZAR cost based on the package size category

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Courier Request successfully created.";
                
                if (User.IsInRole("Client"))
                {
                    return RedirectToAction(nameof(MyRequests));
                }
                else if (User.IsInRole("Driver"))
                {
                    return RedirectToAction(nameof(DriverIndex));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            var contractList = _context.Contracts
                .Include(c => c.Client)
                .Where (c => c.Status == "Active") // filters out the invalid contract statuses so that service requests can only be created for active contracts
                .Select(c => new {
                    Id = c.Id,
                    DisplayText = c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
                });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText", serviceRequest.ContractId);

            // repopulate the driver dropdown if validation fails
            var drivers = await _userManager.GetUsersInRoleAsync("Driver");
            ViewBag.DriverList = new SelectList(drivers, "Id", "UserName", serviceRequest.AssignedDriverId);

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Edit/5
        [Authorize(Roles = "Admin,Staff,Client,Driver")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            await PopulateDriversDropDownList(serviceRequest.AssignedDriverId);

            if (User.IsInRole("Client") && serviceRequest.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending requests.";
                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Driver") && serviceRequest.AssignedDriverId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You can only edit assigned delivery requests.";
                return RedirectToAction(nameof(Index));
            }

            if (serviceRequest == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var contractQuery = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (User.IsInRole("Client"))
            {
                contractQuery = contractQuery.Where(c => c.Client.ApplicationUserId == userId);
            }

            var contractList = contractQuery.Select(c => new {
                Id = c.Id,
                DisplayText = c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
            });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // POST: ServiceRequest/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff,Client,Driver")]
        public async Task<IActionResult> Edit(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            var requestToUpdate = await _context.ServiceRequests.FindAsync(id);
            
            if (requestToUpdate == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client") && serviceRequest.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending requests.";
                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Driver") && serviceRequest.AssignedDriverId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You can only edit assigned delivery requests.";
                return RedirectToAction(nameof(Index));
            }

            requestToUpdate.Description = serviceRequest.Description;
            requestToUpdate.PickupAddress = serviceRequest.PickupAddress;
            requestToUpdate.DeliveryAddress = serviceRequest.DeliveryAddress;
            requestToUpdate.RecipientName = serviceRequest.RecipientName;
            requestToUpdate.RecipientPhone = serviceRequest.RecipientPhone;
            requestToUpdate.PackageSizeCategory = serviceRequest.PackageSizeCategory;
            requestToUpdate.SlaType = serviceRequest.SlaType;
            requestToUpdate.Status = serviceRequest.Status;

            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                requestToUpdate.AssignedDriverId = serviceRequest.AssignedDriverId;
            }

            if (requestToUpdate.OriginalCost != serviceRequest.OriginalCost)
            {
                requestToUpdate.OriginalCost = serviceRequest.OriginalCost;
                requestToUpdate.Cost = await _currencyStrategy.ConvertAsync(serviceRequest.OriginalCost);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Service Request updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                if (User.IsInRole("Client"))
                {
                    return RedirectToAction(nameof(MyRequests));
                }
                else if (User.IsInRole("Driver"))
                {
                    return RedirectToAction(nameof(DriverIndex));
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            await PopulateDriversDropDownList(serviceRequest.AssignedDriverId);

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        // POST: ServiceRequest/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var serviceRequest = await _context.ServiceRequests.FindAsync(id);
                
                if (serviceRequest != null)
                {
                    _context.ServiceRequests.Remove(serviceRequest);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Service Request removed successfully.";
                }
                else
                {
                    TempData["Error"] = "Service Request not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while trying to delete the service request: " + ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.Id == id);
        }

        private async Task PopulateDriversDropDownList(object selectedDriver = null) // helper method to populate the driver dropdown in the views
        {
            var drivers = await _userManager.GetUsersInRoleAsync("Driver");
            ViewBag.DriverList = new SelectList(drivers, "Id", "UserName", selectedDriver);
        }
    }
}
