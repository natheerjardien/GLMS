using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<Client> CreateAsync(Client client);
        Task<bool> UpdateAsync(int id, Client client);
        Task<bool> DeleteAsync(int id);
    }
}
