using CreditFlowAPI.Base.Identity;

namespace CreditFlowAPI.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; } 
        public string? Details { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
