using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Contract>> GetFilteredAsync(string? status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Contract?> GetByIdAsync(int id) =>
            await _context.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Contract?> GetWithServiceRequestsAsync(int id) =>
            await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Contract> AddAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task UpdateAsync(Contract contract)
        {
            _context.Entry(contract).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public async Task DeleteAsync(Contract contract)
        {
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
        }
    }
}
