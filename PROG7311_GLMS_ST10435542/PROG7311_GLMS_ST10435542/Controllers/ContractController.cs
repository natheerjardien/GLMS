using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

// ONLY import MVC models, NEVER the API models here
using PROG7311_GLMS_ST10435542.Models;
using PROG7311_GLMS_ST10435542.Services;
using PROG7311_GLMS_ST10435542.Services.ApiClients;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ContractController : Controller
    {
        private readonly ContractApiClient _apiClient;
        private readonly ClientApiClient _clientApiClient;

        public ContractController(ContractApiClient apiClient, ClientApiClient clientApiClient)
        {
            _apiClient = apiClient;
            _clientApiClient = clientApiClient;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? searchStatus)
        {
            try
            {
                IEnumerable<Contract> contracts;

                if (User.IsInRole("Admin"))
                {
                    // The filter values are forwarded to GET /api/contracts as query string
                    // parameters - the filtering itself happens in the API with LINQ.
                    contracts = await _apiClient.GetAllAsync(searchStatus, startDate, endDate);

                    ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
                    ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
                    ViewData["CurrentStatus"] = searchStatus;
                }
                else
                {
                    contracts = await _apiClient.GetAllAsync();
                }

                return View(contracts);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not reach the GLMS API.";
                return View(Enumerable.Empty<Contract>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            var clients = await _clientApiClient.GetAllAsync();
            ViewData["ClientId"] = new SelectList(clients, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,StartDate,EndDate,ServiceLevel,ContractFile")] Contract contract)
        {
            ModelState.Remove("Status");
            ModelState.Remove("SignedAgreementFilePath");
            ModelState.Remove("Client");
            ModelState.Remove("ServiceRequests");

            if (ModelState.IsValid)
            {
                try
                {
                    if (contract.ContractFile != null && contract.ContractFile.Length > 0)
                    {
                        // Part 2 rule still applies: signed agreements must be PDFs, nothing else
                        if (!FileValidator.IsValidPdf(contract.ContractFile))
                        {
                            ModelState.AddModelError("ContractFile", "Only PDF files are allowed for signed agreements.");
                            var clientsRetry = await _clientApiClient.GetAllAsync();
                            ViewData["ClientId"] = new SelectList(clientsRetry, "Id", "Name", contract.ClientId);
                            return View(contract);
                        }

                        // UUID prefix prevents two uploads with the same name overwriting each other
                        string fileName = Guid.NewGuid().ToString() + "_" + contract.ContractFile.FileName;
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/contracts");
                        Directory.CreateDirectory(uploadDir);
                        string filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await contract.ContractFile.CopyToAsync(stream);
                        }
                        contract.SignedAgreementFilePath = "/uploads/contracts/" + fileName;
                    }

                    await _apiClient.CreateAsync(contract);
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

            var clients = await _clientApiClient.GetAllAsync();
            ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            if (contract.Status == "Expired")
            {
                TempData["Error"] = "Expired contracts are strictly read-only and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            var clients = await _clientApiClient.GetAllAsync();
            ViewData["ClientId"] = new SelectList(clients, "Id", "ContactDetails", contract.ClientId);

            return View(contract);
        }

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
                    await _apiClient.UpdateAsync(id, contract);
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

            var clients = await _clientApiClient.GetAllAsync();
            ViewData["ClientId"] = new SelectList(clients, "Id", "ContactDetails", contract.ClientId);
            return View(contract);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiClient.DeleteAsync(id);
                TempData["Success"] = "Contract deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot delete. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _apiClient.UpdateStatusAsync(id, "Activate");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expire(int id)
        {
            try
            {
                await _apiClient.UpdateStatusAsync(id, "Expire");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PutOnHold(int id)
        {
            try
            {
                await _apiClient.UpdateStatusAsync(id, "Hold");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
