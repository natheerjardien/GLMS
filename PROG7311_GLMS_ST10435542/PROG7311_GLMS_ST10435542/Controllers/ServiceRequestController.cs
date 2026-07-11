using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

// ONLY MVC models and API clients here - the frontend no longer touches the database at all.
using PROG7311_GLMS_ST10435542.Models;
using PROG7311_GLMS_ST10435542.Services.ApiClients;

namespace PROG7311_GLMS_ST10435542.Controllers
{
    [Authorize(Roles = "Admin,Staff,Client,Driver")]
    public class ServiceRequestController : Controller
    {
        private readonly ServiceRequestClient _apiClient;
        private readonly ContractApiClient _contractApiClient;
        private readonly UsersApiClient _usersApiClient;

        public ServiceRequestController(ServiceRequestClient apiClient, ContractApiClient contractApiClient, UsersApiClient usersApiClient)
        {
            _apiClient = apiClient;
            _contractApiClient = contractApiClient;
            _usersApiClient = usersApiClient;
        }

        // GET: ServiceRequest
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _apiClient.GetAllAsync();
                await LoadDriverDictAsync();
                return View(list);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not reach the GLMS API.";
                return View(new List<ServiceRequest>());
            }
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyRequests() // view for the client only
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allRequests = await _apiClient.GetAllAsync();

            var myRequests = allRequests
                .Where(s => s.Client?.ApplicationUserId == userId)
                .ToList();

            await LoadDriverDictAsync();
            return View("MyRequests", myRequests);
        }

        [Authorize(Roles = "Admin,Staff,Driver")]
        public async Task<IActionResult> DriverIndex()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allRequests = await _apiClient.GetAllAsync();

            var assignedRequests = allRequests
                .Where(s => s.AssignedDriverId == userId)
                .ToList();

            await LoadDriverDictAsync();
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

            var serviceRequest = await _apiClient.GetByIdAsync(id.Value);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            await LoadDriverDictAsync();
            return View(serviceRequest);
        }

        // GET: ServiceRequest/Create
        [Authorize(Roles = "Admin,Staff,Client")]
        public async Task<IActionResult> Create()
        {
            await PopulateContractsDropDownListAsync();
            await PopulateDriversDropDownListAsync();
            return View();
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff,Client")]
        public async Task<IActionResult> Create([Bind("Id,ContractId,Description,OriginalCost,PickupAddress,DeliveryAddress,RecipientName,RecipientPhone,SlaType,PackageSizeCategory,AssignedDriverId")] ServiceRequest serviceRequest)
        {
            ModelState.Remove("Contract");
            ModelState.Remove("Status");   // set by the API ("Pending")
            ModelState.Remove("Cost");     // calculated by the API using the live exchange rate
            ModelState.Remove("RequestDate");
            ModelState.Remove("AssignedDriverId");
            ModelState.Remove("Client");

            if (User.IsInRole("Client"))
            {
                serviceRequest.AssignedDriverId = null; // clients cannot assign drivers
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // The API enforces the business rule (only ACTIVE contracts) and computes the
                    // ZAR cost - the frontend just forwards the request and shows the outcome.
                    await _apiClient.CreateAsync(serviceRequest);

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
                catch (InvalidOperationException ex)
                {
                    // e.g. "Cannot create a service request for a missing, Draft, Expired or On Hold contract."
                    ModelState.AddModelError("ContractId", ex.Message);
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Could not reach the GLMS API.");
                }
            }

            await PopulateContractsDropDownListAsync(serviceRequest.ContractId);
            await PopulateDriversDropDownListAsync(serviceRequest.AssignedDriverId);

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

            var serviceRequest = await _apiClient.GetByIdAsync(id.Value);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client") && serviceRequest.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending requests.";
                return RedirectToAction(nameof(MyRequests));
            }

            if (User.IsInRole("Driver") && serviceRequest.AssignedDriverId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You can only edit assigned delivery requests.";
                return RedirectToAction(nameof(DriverIndex));
            }

            await PopulateContractsDropDownListAsync(serviceRequest.ContractId, includeAllStatuses: true);
            await PopulateDriversDropDownListAsync(serviceRequest.AssignedDriverId);

            return View(serviceRequest);
        }

        // POST: ServiceRequest/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff,Client,Driver")]
        public async Task<IActionResult> Edit(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            var requestToUpdate = await _apiClient.GetByIdAsync(id);

            if (requestToUpdate == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Client") && serviceRequest.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending requests.";
                return RedirectToAction(nameof(MyRequests));
            }

            if (User.IsInRole("Driver") && serviceRequest.AssignedDriverId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "You can only edit assigned delivery requests.";
                return RedirectToAction(nameof(DriverIndex));
            }

            requestToUpdate.Description = serviceRequest.Description;
            requestToUpdate.PickupAddress = serviceRequest.PickupAddress;
            requestToUpdate.DeliveryAddress = serviceRequest.DeliveryAddress;
            requestToUpdate.RecipientName = serviceRequest.RecipientName;
            requestToUpdate.RecipientPhone = serviceRequest.RecipientPhone;
            requestToUpdate.PackageSizeCategory = serviceRequest.PackageSizeCategory;
            requestToUpdate.SlaType = serviceRequest.SlaType;
            requestToUpdate.Status = serviceRequest.Status;
            requestToUpdate.OriginalCost = serviceRequest.OriginalCost; // if changed, the API recalculates the ZAR cost

            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                requestToUpdate.AssignedDriverId = serviceRequest.AssignedDriverId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiClient.UpdateAsync(id, requestToUpdate);
                    TempData["Success"] = "Service Request updated successfully.";
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Error"] = "Update failed. " + ex.Message;
                }
                catch (HttpRequestException)
                {
                    TempData["Error"] = "Could not reach the GLMS API.";
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

            await PopulateContractsDropDownListAsync(serviceRequest.ContractId, includeAllStatuses: true);
            await PopulateDriversDropDownListAsync(serviceRequest.AssignedDriverId);

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

            var serviceRequest = await _apiClient.GetByIdAsync(id.Value);

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
                await _apiClient.DeleteAsync(id);

                TempData["Success"] = "Service Request removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while trying to delete the service request: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Contract dropdown, sourced from the API. Only ACTIVE contracts are offered when creating,
        // and clients only ever see their own contracts.
        private async Task PopulateContractsDropDownListAsync(int? selectedContract = null, bool includeAllStatuses = false)
        {
            var contracts = await _contractApiClient.GetAllAsync(status: includeAllStatuses ? null : "Active");

            if (User.IsInRole("Client"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                contracts = contracts.Where(c => c.Client?.ApplicationUserId == userId);
            }

            var contractList = contracts.Select(c => new
            {
                c.Id,
                DisplayText = $"{c.Client?.Name} - {c.ServiceLevel} ({c.Status})"
            });

            ViewData["ContractId"] = new SelectList(contractList, "Id", "DisplayText", selectedContract);
        }

        private async Task PopulateDriversDropDownListAsync(object? selectedDriver = null)
        {
            var drivers = await _usersApiClient.GetDriversAsync();
            ViewBag.DriverList = new SelectList(drivers, "Id", "UserName", selectedDriver);
        }

        private async Task LoadDriverDictAsync()
        {
            var drivers = await _usersApiClient.GetDriversAsync();
            ViewBag.DriverDict = drivers.ToDictionary(d => d.Id, d => d.UserName);
        }
    }
}
