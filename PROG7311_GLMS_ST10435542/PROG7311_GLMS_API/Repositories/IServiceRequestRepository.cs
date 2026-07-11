using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Repositories
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest> AddAsync(ServiceRequest serviceRequest);
        Task UpdateAsync(ServiceRequest serviceRequest);
        Task DeleteAsync(ServiceRequest serviceRequest);
    }
}
