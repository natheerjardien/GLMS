using PROG7311_GLMS_ST10435542.Models; // Pointing specifically to MVC models
using System.Net;
using System.Net.Http.Json;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class ContractApiClient
    {
        private readonly HttpClient _httpClient;
        public ContractApiClient(HttpClient httpClient) => _httpClient = httpClient;

        // Filtering is done by the API (server-side LINQ), the frontend just forwards the query string
        public async Task<IEnumerable<Contract>> GetAllAsync(string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = new List<string>();

            if (!string.IsNullOrEmpty(status))
            {
                query.Add($"status={Uri.EscapeDataString(status)}");
            }

            if (startDate.HasValue)
            {
                query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            }

            if (endDate.HasValue)
            {
                query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            }

            var url = "api/Contracts" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);

            return await _httpClient.GetFromJsonAsync<IEnumerable<Contract>>(url) ?? new List<Contract>();
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Contracts/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Contract>();
        }

        public async Task CreateAsync(Contract contract)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Contracts", contract);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task UpdateAsync(int id, Contract contract)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Contracts/{id}", contract);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task UpdateStatusAsync(int id, string action)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/Contracts/{id}/status", action);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Contracts/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }
    }
}
