using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_ST10435542.Models;

namespace PROG7311_GLMS_ST10435542.Data
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

            // Fixes the decimal precision warnings
            builder.Entity<ServiceRequest>()
                .Property(s => s.OriginalCost)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceRequest>()
                .Property(s => s.Cost)
                .HasColumnType("decimal(18,2)");
        }
    }
}