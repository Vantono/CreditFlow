using CreditFlowAPI.Base.Persistance;
using CreditFlowAPI.Domain.Entities;

namespace CreditFlowAPI.Base.Service
{
    // 1. Το Interface
    public interface IAuditService
    {
        Task LogAsync(string userId, string action, string? details = null);
    }

    // 2. Η Υλοποίηση
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string action, string? details = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
