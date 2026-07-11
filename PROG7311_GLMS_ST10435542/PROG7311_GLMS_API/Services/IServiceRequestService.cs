using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int id);
        Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest);
        Task<bool> UpdateAsync(int id, ServiceRequest serviceRequest);
        Task<bool> DeleteAsync(int id);
    }
}
