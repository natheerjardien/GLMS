using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PROG7311_GLMS_API.Data;

namespace PROG7311_GLMS_ST10435542.Tests
{
    // Boots the real API in memory for integration testing, but swaps SQL Server out for the
    // EF Core InMemory provider so the test suite runs anywhere (CI included) without a database
    // server. Everything else - controllers, service layer, repositories, JWT auth, seeding -
    // is the genuine production pipeline.
    public class CustomWebApplicationFactory : WebApplicationFactory<PROG7311_GLMS_API.Program>
    {
        private readonly string _databaseName = $"GLMS_IntegrationTests_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Strip every EF Core registration that points at SQL Server
                var efDescriptors = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") == true)
                    .ToList();

                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}
