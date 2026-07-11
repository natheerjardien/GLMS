using System.Net.Http.Json;
using PROG7311_GLMS_ST10435542.Models.Auth;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class UsersApiClient
    {
        private readonly HttpClient _httpClient;

        public UsersApiClient(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<List<UserDto>> GetDriversAsync() =>
            await _httpClient.GetFromJsonAsync<List<UserDto>>("api/Users/drivers") ?? new();

        public async Task DeleteUserAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"api/Users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
