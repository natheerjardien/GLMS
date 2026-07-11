using System.Net.Http.Json;
using PROG7311_GLMS_ST10435542.Models.Auth;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient) => _httpClient = httpClient;

        // Returns null when the credentials are wrong; throws HttpRequestException when the API is down
        public async Task<LoginResponseDto?> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { email, password });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        }

        public async Task RegisterClientAsync(RegisterClientRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register-client", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
