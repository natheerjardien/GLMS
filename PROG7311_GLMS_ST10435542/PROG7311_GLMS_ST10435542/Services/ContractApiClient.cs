using PROG7311_GLMS_ST10435542.Models; // Pointing specifically to MVC models
using System.Net.Http.Json;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class ContractApiClient
    {
        private readonly HttpClient _httpClient;
        public ContractApiClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<IEnumerable<Contract>> GetAllAsync() => 
            await _httpClient.GetFromJsonAsync<IEnumerable<Contract>>("api/Contracts") ?? new List<Contract>();

        public async Task<Contract?> GetByIdAsync(int id) => 
            await _httpClient.GetFromJsonAsync<Contract>($"api/Contracts/{id}");

        public async Task CreateAsync(Contract contract) => 
            await _httpClient.PostAsJsonAsync("api/Contracts", contract);

        public async Task UpdateAsync(int id, Contract contract) => 
            await _httpClient.PutAsJsonAsync($"api/Contracts/{id}", contract);

        public async Task UpdateStatusAsync(int id, string action)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/Contracts/{id}/status", action);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Contracts/{id}");
            if (!response.IsSuccessStatusCode) throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}