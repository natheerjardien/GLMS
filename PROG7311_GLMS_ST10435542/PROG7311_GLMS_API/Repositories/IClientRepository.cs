using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Repositories
{
    // Repository pattern: hides all EF Core / SQL details from the rest of the application.
    // Controllers and services only ever talk to these interfaces, never to the DbContext.
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<Client> AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(Client client);
    }
}
