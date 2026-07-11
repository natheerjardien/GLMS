using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using PROG7311_GLMS_API.Models;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http;

namespace PROG7311_GLMS_ST10435542.Tests
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<PROG7311_GLMS_API.Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<PROG7311_GLMS_API.Program> factory)
        {
            // This spins up a test version of the API in memory
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ApiContracts_ReturnsSuccessStatusCodeAndNotNull()
        {
            // Act: Call the API endpoint
            var response = await _client.GetAsync("/api/Contracts");

            // Assert 1: Check that the HTTP Status Code is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert 2: Verify the returned JSON is not null
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            Assert.NotNull(contracts);
        }

        [Fact]
        public async Task Get_ApiClients_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/Clients");

            // Assert
            response.EnsureSuccessStatusCode(); // Throws if not 200-299
            var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
            Assert.NotNull(clients);
        }
    }
}