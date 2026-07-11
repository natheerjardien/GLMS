using System.Net.Http.Json;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class ServiceRequestClient
    {
        private readonly HttpClient _httpClient;

        public ServiceRequestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri("http://localhost:5187/"); 
        }

        public async Task<List<ServiceRequest>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ServiceRequest>>("api/ServiceRequests") ?? new();
        }

        public async Task<ServiceRequest?> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ServiceRequest>($"api/ServiceRequests/{id}");
        }

        public async Task CreateAsync(ServiceRequest sr)
        {
            await _httpClient.PostAsJsonAsync("api/ServiceRequests", sr);

        }

        public async Task UpdateAsync(int id, ServiceRequest sr) 
        {
            await _httpClient.PutAsJsonAsync($"api/ServiceRequests/{id}", sr);
        }

        public async Task DeleteAsync(int id) 
        {
            await _httpClient.DeleteAsync($"api/ServiceRequests/{id}");
        }
    }
}