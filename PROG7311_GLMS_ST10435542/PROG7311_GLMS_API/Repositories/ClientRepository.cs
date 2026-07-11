using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Client>> GetAllAsync() =>
            await _context.Clients.Include(c => c.Contracts).ToListAsync();

        public async Task<Client?> GetByIdAsync(int id) =>
            await _context.Clients.Include(c => c.Contracts).FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Client> AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task UpdateAsync(Client client)
        {
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }
    }
}
