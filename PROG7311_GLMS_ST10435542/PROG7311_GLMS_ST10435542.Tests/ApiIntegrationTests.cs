using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_ST10435542.Tests
{
    // Automated integration tests: these boot the REAL API (controllers -> services ->
    // repositories -> EF Core) in memory and fire genuine HTTP requests at it, exactly like
    // a CI/CD pipeline would before a deployment.
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ApiIntegrationTests(CustomWebApplicationFactory factory) => _factory = factory;

        private record LoginResult(string Token, DateTime Expires, string UserId, string Email, List<string> Roles);

        // Logs in through the real /api/auth/login endpoint and returns a client
        // that sends the JWT with every request - the same flow the MVC frontend uses.
        private async Task<HttpClient> CreateAuthenticatedClientAsync()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/Auth/login",
                new { email = "admin@glms.com", password = "admin123" });
            response.EnsureSuccessStatusCode();

            var login = await response.Content.ReadFromJsonAsync<LoginResult>();
            Assert.NotNull(login);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
            return client;
        }

        private static async Task<Client> CreateClientRecordAsync(HttpClient client)
        {
            var response = await client.PostAsJsonAsync("/api/Clients", new Client
            {
                Name = "Integration Test Client",
                ContactDetails = "011 555 0000",
                Region = "Gauteng"
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<Client>())!;
        }

        private static async Task<Contract> CreateContractAsync(HttpClient client, int clientId)
        {
            var response = await client.PostAsJsonAsync("/api/Contracts", new Contract
            {
                ClientId = clientId,
                ServiceLevel = "Premium",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(12)
            });

            Assert.True(response.StatusCode == HttpStatusCode.Created,
                $"POST /api/Contracts failed ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
            return (await response.Content.ReadFromJsonAsync<Contract>())!;
        }

        // -------------------- Authentication --------------------

        [Fact]
        public async Task Get_ApiContracts_WithoutToken_Returns401Unauthorized()
        {
            var client = _factory.CreateClient(); // no JWT attached

            var response = await client.GetAsync("/api/Contracts");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Post_Login_WithValidCredentials_ReturnsTokenAndRoles()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/Auth/login",
                new { email = "admin@glms.com", password = "admin123" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var login = await response.Content.ReadFromJsonAsync<LoginResult>();
            Assert.NotNull(login);
            Assert.False(string.IsNullOrEmpty(login!.Token));
            Assert.Contains("Admin", login.Roles);
        }

        [Fact]
        public async Task Post_Login_WithWrongPassword_Returns401Unauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/Auth/login",
                new { email = "admin@glms.com", password = "definitely-wrong" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // -------------------- Basic endpoint health --------------------

        [Fact]
        public async Task Get_ApiContracts_ReturnsSuccessStatusCodeAndNotNull()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/Contracts");

            // Assert 1: HTTP status code is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert 2: the returned JSON is not null
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            Assert.NotNull(contracts);
        }

        [Fact]
        public async Task Get_ApiClients_ReturnsSuccessStatusCode()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.GetAsync("/api/Clients");

            response.EnsureSuccessStatusCode();
            var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
            Assert.NotNull(clients);
        }

        // -------------------- Data integrity: create, then read it back --------------------

        [Fact]
        public async Task Post_Contract_CreateThenRead_ReturnsSameData()
        {
            var client = await CreateAuthenticatedClientAsync();
            var testClient = await CreateClientRecordAsync(client);

            var created = await CreateContractAsync(client, testClient.Id);

            // The factory must have forced the new contract into Draft
            Assert.Equal("Draft", created.Status);

            // Read it back through the API and verify the data survived the round trip
            var fetched = await client.GetFromJsonAsync<Contract>($"/api/Contracts/{created.Id}");

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched!.Id);
            Assert.Equal(testClient.Id, fetched.ClientId);
            Assert.Equal("Premium", fetched.ServiceLevel);
            Assert.Equal("Draft", fetched.Status);
        }

        [Fact]
        public async Task Get_Contracts_FilterByStatus_OnlyReturnsMatchingContracts()
        {
            var client = await CreateAuthenticatedClientAsync();
            var testClient = await CreateClientRecordAsync(client);

            var draft = await CreateContractAsync(client, testClient.Id);
            var activated = await CreateContractAsync(client, testClient.Id);
            await client.PatchAsJsonAsync($"/api/Contracts/{activated.Id}/status", "Activate");

            var activeContracts = await client.GetFromJsonAsync<List<Contract>>("/api/Contracts?status=Active");

            Assert.NotNull(activeContracts);
            Assert.All(activeContracts!, c => Assert.Equal("Active", c.Status));
            Assert.Contains(activeContracts!, c => c.Id == activated.Id);
            Assert.DoesNotContain(activeContracts!, c => c.Id == draft.Id);
        }

        // -------------------- Workflow rules over HTTP --------------------

        [Fact]
        public async Task Patch_ContractStatus_ActivateDraft_ReturnsOkAndActiveStatus()
        {
            var client = await CreateAuthenticatedClientAsync();
            var testClient = await CreateClientRecordAsync(client);
            var contract = await CreateContractAsync(client, testClient.Id);

            var response = await client.PatchAsJsonAsync($"/api/Contracts/{contract.Id}/status", "Activate");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<Contract>();
            Assert.Equal("Active", updated!.Status);
        }

        [Fact]
        public async Task Patch_ContractStatus_HoldOnDraft_ReturnsBadRequest()
        {
            var client = await CreateAuthenticatedClientAsync();
            var testClient = await CreateClientRecordAsync(client);
            var contract = await CreateContractAsync(client, testClient.Id);

            // The State pattern forbids Draft -> On Hold; the API must translate that into a 400
            var response = await client.PatchAsJsonAsync($"/api/Contracts/{contract.Id}/status", "Hold");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Patch_ContractStatus_UnknownContract_Returns404NotFound()
        {
            var client = await CreateAuthenticatedClientAsync();

            var response = await client.PatchAsJsonAsync("/api/Contracts/999999/status", "Activate");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_ServiceRequest_OnExpiredContract_ReturnsBadRequest()
        {
            var client = await CreateAuthenticatedClientAsync();
            var testClient = await CreateClientRecordAsync(client);
            var contract = await CreateContractAsync(client, testClient.Id);

            await client.PatchAsJsonAsync($"/api/Contracts/{contract.Id}/status", "Activate");
            await client.PatchAsJsonAsync($"/api/Contracts/{contract.Id}/status", "Expire");

            var response = await client.PostAsJsonAsync("/api/ServiceRequests", BuildServiceRequest(contract.Id));

            // Core business rule: no service requests against Expired contracts
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ServiceRequest_OnActiveContract_Returns201WithCalculatedCost()
        {
            var client = await CreateAuthenticatedClientAsync();
            var testClient = await CreateClientRecordAsync(client);
            var contract = await CreateContractAsync(client, testClient.Id);
            await client.PatchAsJsonAsync($"/api/Contracts/{contract.Id}/status", "Activate");

            var response = await client.PostAsJsonAsync("/api/ServiceRequests", BuildServiceRequest(contract.Id));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<ServiceRequest>();
            Assert.NotNull(created);
            Assert.Equal("Pending", created!.Status);
            Assert.Equal(testClient.Id, created.ClientId);      // linked to the contract's client by the API
            Assert.True(created.Cost > 0);                      // ZAR cost calculated server-side
        }

        private static ServiceRequest BuildServiceRequest(int contractId) => new()
        {
            ContractId = contractId,
            Description = "Integration test delivery",
            OriginalCost = 0m,
            Cost = 0m,
            PickupAddress = "1 Warehouse Road, Johannesburg",
            DeliveryAddress = "99 Harbour View, Cape Town",
            RecipientName = "Test Recipient",
            RecipientPhone = "082 555 0000",
            SlaType = "Express",
            PackageSizeCategory = "5-10kg"
        };
    }
}
