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
    [Authorize(Roles = "Admin,Staff")] // both admin and staff can access this controller
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractFactory _contractFactory;
        private readonly IContractStateManager _stateManager;

        public ContractController(ApplicationDbContext context, IContractFactory contractFactory, IContractStateManager stateManager)
        {
            _context = context;
            _contractFactory = contractFactory;
            _stateManager = stateManager;
        }

        // GET: Contract
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string searchStatus)
        {
            var contracts = from c in _context.Contracts.Include(c => c.Client) select c; // pulls all the clients and their contracts from the DB

            if (User.IsInRole("Admin"))
            {
                if (startDate.HasValue)
                {
                    contracts = contracts.Where(c => c.StartDate >= startDate.Value); // filters contracts to only include those that start on or after the chosen start date
                }

                if (endDate.HasValue)
                {
                    contracts = contracts.Where(c => c.EndDate <= endDate.Value); // filters contracts to only include those that end on or before the chosen end date
                }

                if (!string.IsNullOrEmpty(searchStatus))
                {
                    contracts = contracts.Where(c => c.Status == searchStatus); // filters contracts to only include those with the chosen status
                }

                ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
                ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
                ViewData["CurrentStatus"] = searchStatus;
            }

            return View(await contracts.ToListAsync());
        }

        // GET: Contract/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contract/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name");
            return View();
        }

        // POST: Contract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,StartDate,EndDate,ServiceLevel,ContractFile")] Contract contract) // dont have to include Status and SignedAgreementFilePath because they are set in the factory
        {
            ModelState.Remove("Status"); // remove validation for Status since it will be set in the factory
            ModelState.Remove("SignedAgreementFilePath"); // removed since it will be set in the factory
            ModelState.Remove("Client");
            ModelState.Remove("ServiceRequests");

            if (ModelState.IsValid)
            {
                var newContract = _contractFactory.CreateContract(
                    contract.ClientId,
                    contract.ServiceLevel,
                    contract.StartDate,
                    contract.EndDate
                ); // creates a new contract using the factory which sets the default values for Status and SignedAgreementFilePath

                if (contract.ContractFile != null && contract.ContractFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + contract.ContractFile.FileName;
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/contracts", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await contract.ContractFile.CopyToAsync(stream);
                    }

                    newContract.SignedAgreementFilePath = "/uploads/contracts/" + fileName;
                }

                _context.Add(newContract);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contract/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            if (contract.Status == "Expired") // for just in case the user tries to edit through the url even though the button is hidden for expired contracts
            {
                TempData["Error"] = "Expired contracts are strictly read-only and cannot be edited.";
                
                return RedirectToAction(nameof(Index)); 
            }

            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
            
            return View(contract);
        }

        // POST: Contract/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,ServiceLevel,Status,SignedAgreementFilePath")] Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
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
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        // GET: Contract/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contract/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (contract == null)
                {
                    TempData["Error"] = "Contract not found.";

                    return RedirectToAction(nameof(Index));
                }

                if (contract.ServiceRequests.Any())
                {
                    TempData["Error"] = "Oh snap! This contract has Service Requests linked to it. You must remove those requests before deleting the contract.";
                    
                    return RedirectToAction(nameof(Index));
                }

                _context.Contracts.Remove(contract);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete this contract because it has active Service Requests linked to it. Delete the requests first!";
            }
                        
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> Activate (int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                _stateManager.ActivateContract(contract);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // this catches the "already active" errors
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Expire (int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                _stateManager.ExpireContract(contract);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PutOnHold(int id) // added this new method to allow admin to put a contract on hold which prevents new service requests from being created for that contract until its reactivated
        {
            var contract = await _context.Contracts.FindAsync(id);
            
            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                _stateManager.PutOnHoldContract(contract);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message; // this catches the "already on hold" errors
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
