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
}
