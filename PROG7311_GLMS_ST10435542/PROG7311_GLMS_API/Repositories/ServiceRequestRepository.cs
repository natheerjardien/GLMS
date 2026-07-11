using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Repositories
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync() =>
            await _context.ServiceRequests
                .Include(s => s.Contract)
                .Include(s => s.Client)
                .ToListAsync();

        public async Task<ServiceRequest?> GetByIdAsync(int id) =>
            await _context.ServiceRequests
                .Include(s => s.Contract)
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<ServiceRequest> AddAsync(ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
            return serviceRequest;
        }

        public async Task UpdateAsync(ServiceRequest serviceRequest)
        {
            _context.Entry(serviceRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ServiceRequest serviceRequest)
        {
            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();
        }
    }
}
