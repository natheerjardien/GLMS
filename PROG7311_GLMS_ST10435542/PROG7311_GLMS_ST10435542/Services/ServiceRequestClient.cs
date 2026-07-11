using System.Net;
using System.Net.Http.Json;
using PROG7311_GLMS_ST10435542.Models; // MVC's own models - no dependency on the API project

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class ServiceRequestClient
    {
        private readonly HttpClient _httpClient;

        // BaseAddress comes from configuration (ApiSettings:BaseUrl) via Program.cs,
        // so Docker can point this at the API container without a code change.
        public ServiceRequestClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<List<ServiceRequest>> GetAllAsync() =>
            await _httpClient.GetFromJsonAsync<List<ServiceRequest>>("api/ServiceRequests") ?? new();

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/ServiceRequests/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ServiceRequest>();
        }

        public async Task CreateAsync(ServiceRequest sr)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ServiceRequests", sr);

            // Surfaces the API's business-rule message (e.g. "contract is Expired") to the caller
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task UpdateAsync(int id, ServiceRequest sr)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/ServiceRequests/{id}", sr);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ServiceRequests/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }
    }
}
