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

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ServiceRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyConversionStrategy _currencyStrategy;

        public ServiceRequestController(ApplicationDbContext context, ICurrencyConversionStrategy currencyStrategy)
        {
            _context = context;
            _currencyStrategy = currencyStrategy;
        }

        // GET: ServiceRequest
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServiceRequest/Details/5
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
        public IActionResult Create()
        {
            var contractList = _context.Contracts
            .Include(c => c.Client)
            .Where(c => c.Status != "Expired" && c.Status != "On Hold") // filters out the invalid contract statuses so that service requests can only be created for active contracts
            .Select(c => new {
                Id = c.Id,
                DisplayText = c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
            });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText");

            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ContractId,Description,OriginalCost")] ServiceRequest serviceRequest)
        {
            ModelState.Remove("Contract"); // remove validation for Contract since we are only binding the ContractId and not the entire Contract object
            ModelState.Remove("Status"); // removes validation for Status since we will set it to "Pending" by default in the controller    
            ModelState.Remove("Cost");

            var parentContract = await _context.Contracts.FindAsync(serviceRequest.ContractId);

            if (parentContract != null && (parentContract.Status == "Expired" || parentContract.Status == "On Hold"))
            {
                ModelState.AddModelError("ContractId", "Cannot create a service request for an Expired or On Hold contract.");
            }

            if (ModelState.IsValid)
            {
                serviceRequest.Status = "Pending";

                serviceRequest.Cost = await _currencyStrategy.ConvertAsync(serviceRequest.OriginalCost); // converts the cost to rands before saving to the DB

                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }

            var contractList = _context.Contracts
            .Include(c => c.Client)
            .Where (c => c.Status != "Expired" && c.Status != "On Hold") // filters out the invalid contract statuses so that service requests can only be created for active contracts
            .Select(c => new {
                Id = c.Id,
                DisplayText = c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
            });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText", serviceRequest.ContractId);

            return View(serviceRequest);
        }

        // GET: ServiceRequest/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            var contractList = _context.Contracts.Include(c => c.Client).Select(c => new {
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,ContractId,Description,OriginalCost,Status")] ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Contract");
            ModelState.Remove("Cost"); // this gets calculated so doenst need validaton

            if (ModelState.IsValid)
            {
                try
                {
                    serviceRequest.Cost = await _currencyStrategy.ConvertAsync(serviceRequest.OriginalCost); // recalculates the rand price based off the updated dollar value entered

                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }

            // repopulate the dropdowns if validation fails
            var contractList = _context.Contracts.Include(c => c.Client).Select(c => new {
                Id = c.Id,
                DisplayText = c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
            });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText", serviceRequest.ContractId);

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
    }
}
