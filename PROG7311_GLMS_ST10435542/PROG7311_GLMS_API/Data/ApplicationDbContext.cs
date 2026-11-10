using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // This is CRITICAL for Identity to work

            // Used OnModelCreating to configure EF Core fluent API relationships and prevent cascading delete loops in SQL Server (Microsoft, 2026).
            builder.Entity<Contract>() // creates a strict one-to-many relationship between client and contract
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceRequest>() // creates a strict one-to-many relationship between contract and service request
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Client)
                .WithMany()
                .HasForeignKey(sr => sr.ClientId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
/* Reference List:
    Microsoft. 2026. Creating and configuring a model in Entity Framework Core. [online] Available at: https://learn.microsoft.com/en-us/ef/core/modeling/ [Accessed 22 April 2026].
*/