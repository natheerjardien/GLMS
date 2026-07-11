using PROG7311_GLMS_ST10435542.Models; // Pointing specifically to MVC models
using System.Net;
using System.Net.Http.Json;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class ClientApiClient
    {
        private readonly HttpClient _httpClient;
        public ClientApiClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<IEnumerable<Client>> GetAllAsync() =>
            await _httpClient.GetFromJsonAsync<IEnumerable<Client>>("api/Clients") ?? new List<Client>();

        public async Task<Client?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Clients/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Client>();
        }

        public async Task CreateAsync(Client client)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Clients", client);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task UpdateAsync(int id, Client client)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Clients/{id}", client);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Clients/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }
    }
}
