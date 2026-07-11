using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Repositories;

namespace PROG7311_GLMS_API.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IPricingService _pricingService;                     // uses the Strategy pattern internally for currency conversion
        private readonly ICurrencyConversionStrategy _currencyStrategy;

        public ServiceRequestService(
            IServiceRequestRepository serviceRequestRepository,
            IContractRepository contractRepository,
            IPricingService pricingService,
            ICurrencyConversionStrategy currencyStrategy)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _contractRepository = contractRepository;
            _pricingService = pricingService;
            _currencyStrategy = currencyStrategy;
        }

        public Task<IEnumerable<ServiceRequest>> GetAllAsync() => _serviceRequestRepository.GetAllAsync();

        public Task<ServiceRequest?> GetByIdAsync(int id) => _serviceRequestRepository.GetByIdAsync(id);

        public async Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest)
        {
            if (serviceRequest.ContractId == null)
            {
                throw new InvalidOperationException("A service request must be linked to a contract.");
            }

            var parentContract = await _contractRepository.GetByIdAsync(serviceRequest.ContractId.Value);

            // Core workflow rule from Part 2, now enforced in the backend where it cannot be bypassed:
            // requests may only be raised against valid, ACTIVE contracts.
            if (parentContract == null || parentContract.Status != "Active")
            {
                throw new InvalidOperationException("Cannot create a service request for a missing, Draft, Expired or On Hold contract.");
            }

            serviceRequest.ClientId = parentContract.ClientId;
            serviceRequest.Status = "Pending";
            serviceRequest.RequestDate = DateTime.UtcNow;

            // Server-side pricing: ZAR total comes from the package size + SLA via the currency strategy
            serviceRequest.Cost = await _pricingService.GetTotalCostInUsdAsync(
                serviceRequest.PackageSizeCategory ?? string.Empty,
                serviceRequest.SlaType ?? "Standard");

            return await _serviceRequestRepository.AddAsync(serviceRequest);
        }

        public async Task<bool> UpdateAsync(int id, ServiceRequest serviceRequest)
        {
            var existing = await _serviceRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            existing.Description = serviceRequest.Description;
            existing.PickupAddress = serviceRequest.PickupAddress;
            existing.DeliveryAddress = serviceRequest.DeliveryAddress;
            existing.RecipientName = serviceRequest.RecipientName;
            existing.RecipientPhone = serviceRequest.RecipientPhone;
            existing.PackageSizeCategory = serviceRequest.PackageSizeCategory;
            existing.SlaType = serviceRequest.SlaType;
            existing.Status = serviceRequest.Status;
            existing.AssignedDriverId = serviceRequest.AssignedDriverId;

            // If the USD amount changed, recalculate the ZAR cost with the live exchange rate
            if (serviceRequest.OriginalCost.HasValue && existing.OriginalCost != serviceRequest.OriginalCost)
            {
                existing.OriginalCost = serviceRequest.OriginalCost;
                existing.Cost = await _currencyStrategy.ConvertAsync(serviceRequest.OriginalCost.Value);
            }

            await _serviceRequestRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _serviceRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _serviceRequestRepository.DeleteAsync(existing);
            return true;
        }
    }
}
