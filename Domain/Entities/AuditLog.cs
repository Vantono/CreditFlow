using CreditFlowAPI.Base.Identity;

namespace CreditFlowAPI.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty; // Ποιος το έκανε
        public string Action { get; set; } = string.Empty; // Τι έκανε (π.χ. "VIEW_LOANS")
        public DateTime Timestamp { get; set; } // Πότε το έκανε
        public string? Details { get; set; } // Προαιρετικό: Λεπτομέρειες (π.χ. IP Address)
        public virtual ApplicationUser? User { get; set; }
    }
}
