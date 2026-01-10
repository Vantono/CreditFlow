using CreditFlowAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CreditFlowAPI.Domain.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<LoanApplication> LoanApplications { get; }
        DbSet<LoanDocument> LoanDocuments { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
