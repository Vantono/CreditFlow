using System.ComponentModel.DataAnnotations;

namespace CreditFlowAPI.Domain.Entities
{
    public record RegisterRequest(
        [Required][EmailAddress] string Email,
        [Required] string Password,
        [Required] string FirstName,
        [Required] string LastName,
        [Required] string TaxId,
        [Required] DateTime DateOfBirth,
        [Required] string PhoneNumber,
        [Required] string Street,
        [Required] string City,
        [Required] string State,
        [Required] string ZipCode
    );

    public record LoginRequest(
        [Required][EmailAddress] string Email,
        [Required] string Password
    );

    public record AuthResponse(
        string Token,
        string Email,
        string UserName
    );

    public record CreateLoanRequest(
        [Required][Range(100, 1000000)] decimal LoanAmount,
        [Required][Range(1, 360)] int TermMonths,
        [Required] string Purpose,
        [Required] string EmployerName,
        [Required] string JobTitle,
        [Required][Range(0, 60)] int YearsEmployed,
        [Required][Range(0, 1000000)] decimal MonthlyIncome,
        [Required][Range(0, 1000000)] decimal MonthlyExpenses
    );

    public record SubmitLoanRequest(
        [Required] Guid LoanId
    );

    public record GetLoanDetailsRequest(
        [Required] Guid LoanId
    );

    public record ArchiveLoanRequest(
        [Required] Guid LoanId,
        string? Reason 
    );

    public record LoanDto(
    Guid Id,
    decimal LoanAmount,
    int TermMonths,
    string Purpose,
    string Status,     
    int StatusCode, 
    DateTime CreatedAt,
    string? ApplicantName,
    decimal? InterestRate,
    decimal? MonthlyPayment,
    decimal? TotalInterest,
    decimal? DebtToIncomeRatio,
    string? RiskLevel
);
    public record DecideLoanRequest(
    Guid LoanId,
    bool Approved,
    string? Comments
);

    public record UploadDocumentRequest(
        Guid LoanId,
        IFormFile File
    );
}
