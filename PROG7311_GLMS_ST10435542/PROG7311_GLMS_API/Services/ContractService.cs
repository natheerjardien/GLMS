using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Repositories;

namespace PROG7311_GLMS_API.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IContractFactory _contractFactory;       // Factory pattern (Part 1/2) reused in the backend
        private readonly IContractStateManager _stateManager;     // State pattern (Part 1/2) reused in the backend

        public ContractService(IContractRepository contractRepository, IContractFactory contractFactory, IContractStateManager stateManager)
        {
            _contractRepository = contractRepository;
            _contractFactory = contractFactory;
            _stateManager = stateManager;
        }

        public Task<IEnumerable<Contract>> GetContractsAsync(string? status, DateTime? startDate, DateTime? endDate) =>
            _contractRepository.GetFilteredAsync(status, startDate, endDate);

        public Task<Contract?> GetByIdAsync(int id) => _contractRepository.GetByIdAsync(id);

        public async Task<Contract> CreateAsync(Contract contract)
        {
            if (contract.ClientId == null || contract.StartDate == null || string.IsNullOrEmpty(contract.ServiceLevel))
            {
                throw new InvalidOperationException("A contract needs a client, a start date and a service level.");
            }

            if (contract.EndDate <= contract.StartDate)
            {
                throw new InvalidOperationException("The contract end date must be after the start date.");
            }

            // The factory guarantees every new contract starts life in the Draft state
            var newContract = _contractFactory.CreateContract(
                contract.ClientId.Value,
                contract.ServiceLevel,
                contract.StartDate.Value,
                contract.EndDate);

            newContract.SignedAgreementFilePath = contract.SignedAgreementFilePath;

            return await _contractRepository.AddAsync(newContract);
        }

        public async Task<bool> UpdateAsync(int id, Contract contract)
        {
            var existing = await _contractRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            // Expired contracts are read-only; the state machine treats Expired as terminal
            if (existing.Status == "Expired")
            {
                throw new InvalidOperationException("Expired contracts are read-only and cannot be edited.");
            }

            existing.ClientId = contract.ClientId;
            existing.StartDate = contract.StartDate;
            existing.EndDate = contract.EndDate;
            existing.ServiceLevel = contract.ServiceLevel;
            existing.Status = contract.Status;

            if (!string.IsNullOrEmpty(contract.SignedAgreementFilePath))
            {
                existing.SignedAgreementFilePath = contract.SignedAgreementFilePath;
            }

            await _contractRepository.SaveChangesAsync();
            return true;
        }

        public async Task<Contract?> ChangeStatusAsync(int id, string action)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return null;
            }

            // The state pattern enforces which transitions are legal for the current status
            switch (action)
            {
                case "Activate":
                    _stateManager.ActivateContract(contract);
                    break;
                case "Expire":
                    _stateManager.ExpireContract(contract);
                    break;
                case "Hold":
                    _stateManager.PutOnHoldContract(contract);
                    break;
                default:
                    throw new InvalidOperationException($"'{action}' is not a valid status action. Use Activate, Expire or Hold.");
            }

            await _contractRepository.SaveChangesAsync();
            return contract;
        }

        public async Task DeleteAsync(int id)
        {
            var contract = await _contractRepository.GetWithServiceRequestsAsync(id);
            if (contract == null)
            {
                throw new KeyNotFoundException($"Contract {id} was not found.");
            }

            if (contract.ServiceRequests.Any())
            {
                throw new InvalidOperationException("Cannot delete a contract that has Service Requests linked to it.");
            }

            await _contractRepository.DeleteAsync(contract);
        }
    }
}
