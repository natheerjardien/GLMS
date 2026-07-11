using PROG7311_GLMS_ST10435542.Models; // Pointing specifically to MVC models
using System.Net.Http.Json;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class ClientApiClient
    {
        private readonly HttpClient _httpClient;
        public ClientApiClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<IEnumerable<Client>> GetAllAsync() => 
            await _httpClient.GetFromJsonAsync<IEnumerable<Client>>("api/Clients") ?? new List<Client>();

        public async Task<Client?> GetByIdAsync(int id) => 
            await _httpClient.GetFromJsonAsync<Client>($"api/Clients/{id}");

        public async Task CreateAsync(Client client) => 
            await _httpClient.PostAsJsonAsync("api/Clients", client);

        public async Task UpdateAsync(int id, Client client) => 
            await _httpClient.PutAsJsonAsync($"api/Clients/{id}", client);

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Clients/{id}");
            if (!response.IsSuccessStatusCode) throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}