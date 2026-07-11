using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Repositories;

namespace PROG7311_GLMS_API.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository) => _clientRepository = clientRepository;

        public Task<IEnumerable<Client>> GetAllAsync() => _clientRepository.GetAllAsync();

        public Task<Client?> GetByIdAsync(int id) => _clientRepository.GetByIdAsync(id);

        public Task<Client> CreateAsync(Client client) => _clientRepository.AddAsync(client);

        public async Task<bool> UpdateAsync(int id, Client client)
        {
            var existing = await _clientRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            existing.Name = client.Name;
            existing.ContactDetails = client.ContactDetails;
            existing.Region = client.Region;

            await _clientRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _clientRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _clientRepository.DeleteAsync(existing);
            return true;
        }
    }
}
