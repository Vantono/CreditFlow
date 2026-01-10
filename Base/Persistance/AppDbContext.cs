using CreditFlowAPI.Base.Identity;
using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Base.Persistance
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
        public DbSet<LoanDocument> LoanDocuments => Set<LoanDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LoanApplication>()
                .Property(p => p.LoanAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<LoanApplication>()
                .Property(p => p.RowVersion)
                .IsConcurrencyToken();
        }
    }
}
