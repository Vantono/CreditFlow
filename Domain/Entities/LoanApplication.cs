using CreditFlowAPI.Domain.Enums;

namespace CreditFlowAPI.Domain.Entities
{
    public class LoanApplication
    {
        public Guid Id { get; set; }

        // The user who owns this loan (from Identity Provider)
        public required string ApplicantId { get; set; }

        // Financials - ALWAYS use decimal for money
        public decimal LoanAmount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;

        // Employment Information
        public string? EmployerName { get; set; }
        public string? JobTitle { get; set; }
        public int YearsEmployed { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }

        // Calculated Fields
        public decimal InterestRate { get; set; }  // Annual percentage rate
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public string? RiskLevel { get; set; }  // Low, Medium, High

        // State Management
        public LoanStatus Status { get; set; } = LoanStatus.Draft;

        // Audit Trail
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedOnUtc { get; private set; }
        public string? BankerComments { get; set; }

        // Navigation Property (One Loan has Many Documents)
        public ICollection<LoanDocument> Documents { get; set; } = new List<LoanDocument>();

        // CONCURRENCY TOKEN (Magic Field)
        // EF Core uses this to detect if two people edit at the same time
        public byte[]? RowVersion { get; set; }

        // --- Domain Logic Methods (Rich Domain Model) ---
        // Instead of setting properties directly, we use methods to enforce rules.

        public void Submit()
        {
            if (Status != LoanStatus.Draft)
                throw new InvalidOperationException("Only draft loans can be submitted.");

            if (LoanAmount <= 0)
                throw new InvalidOperationException("Loan amount must be greater than zero.");

            Status = LoanStatus.Submitted;
            SubmittedOnUtc = DateTime.UtcNow;
        }

        public void Approve(string comments)
        {
            if (Status != LoanStatus.Submitted && Status != LoanStatus.UnderReview)
                throw new InvalidOperationException("Μπορείτε να εγκρίνετε μόνο υποβληθείσες αιτήσεις.");

            Status = LoanStatus.Approved;
            BankerComments = comments;
            // Εδώ θα μπορούσαμε να βάλουμε και ApprovedOnUtc = DateTime.UtcNow;
        }

        public void Reject(string comments)
        {
            if (Status != LoanStatus.Submitted && Status != LoanStatus.UnderReview)
                throw new InvalidOperationException("Μπορείτε να απορρίψετε μόνο υποβληθείσες αιτήσεις.");

            Status = LoanStatus.Rejected;
            BankerComments = comments;
        }
    }
}
