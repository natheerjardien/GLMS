using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
    // Service layer: all contract business rules live here, keeping the API controllers thin.
    public interface IContractService
    {
        Task<IEnumerable<Contract>> GetContractsAsync(string? status, DateTime? startDate, DateTime? endDate);
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract> CreateAsync(Contract contract);
        Task<bool> UpdateAsync(int id, Contract contract);
        Task<Contract?> ChangeStatusAsync(int id, string action);
        Task DeleteAsync(int id);
    }
}
