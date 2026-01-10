namespace CreditFlowAPI.Domain.Enums
{
    public enum LoanStatus
    {
        Draft = 1,          // Applicant is editing
        Submitted = 2,      // Locked, waiting for bank
        UnderReview = 3,    // Banker is looking at it
        Approved = 4,       // Final Success
        Rejected = 5        // Final Failure
    }

    public enum UserRole
    {
        Applicant = 1,
        Banker = 2,
        Admin = 3
    }

    public enum AuditAction
    {
        CreateLoanApplication = 1,
        SubmitLoanApplication = 2,
        ReviewLoanApplication = 3,
        ApproveLoanApplication = 4,
        RejectLoanApplication = 5,
        UserLogin = 6,
        UserLogout = 7,
        ViewLoans = 8
    }

    public enum DecisionType { Approve, Reject }
}
