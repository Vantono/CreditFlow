using CreditFlowAPI.Base.Identity;
using CreditFlowAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CreditFlowAPI.Domain.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<LoanApplication> LoanApplications { get; }
        DbSet<LoanDocument> LoanDocuments { get; }
        DbSet<ApplicationUser> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
