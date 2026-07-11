using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Repositories
{
    public interface IContractRepository
    {
        // Filtering happens at the database level with LINQ so we never pull the whole table.
        Task<IEnumerable<Contract>> GetFilteredAsync(string? status, DateTime? startDate, DateTime? endDate);
        Task<Contract?> GetByIdAsync(int id);
        Task<Contract?> GetWithServiceRequestsAsync(int id);
        Task<Contract> AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task SaveChangesAsync();
        Task DeleteAsync(Contract contract);
    }
}
