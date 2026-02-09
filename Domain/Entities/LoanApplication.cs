using CreditFlowAPI.Domain.Enums;

namespace CreditFlowAPI.Domain.Entities
{
    public class LoanApplication
    {
        public Guid Id { get; set; }
        public required string ApplicantId { get; set; }
        public decimal LoanAmount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;

        public string? EmployerName { get; set; }
        public string? JobTitle { get; set; }
        public int YearsEmployed { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal InterestRate { get; set; }  
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public string? RiskLevel { get; set; }  
        public LoanStatus Status { get; set; } = LoanStatus.Draft;

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedOnUtc { get; private set; }
        public string? BankerComments { get; set; }

        public ICollection<LoanDocument> Documents { get; set; } = new List<LoanDocument>();
        public byte[]? RowVersion { get; set; }

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
