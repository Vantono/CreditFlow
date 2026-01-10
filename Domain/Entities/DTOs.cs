using System.ComponentModel.DataAnnotations;

namespace CreditFlowAPI.Domain.Entities
{
    public record RegisterRequest(
        [Required][EmailAddress] string Email,
        [Required] string Password,
        [Required] string FirstName,
        [Required] string LastName
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

    // 1. Δημιουργία (Create)
    public record CreateLoanRequest(
        [Required][Range(100, 1000000)] decimal LoanAmount,
        [Required][Range(1, 360)] int TermMonths,
        [Required] string Purpose
    );

    // 2. Υποβολή (Submit)
    public record SubmitLoanRequest(
        [Required] Guid LoanId
    );

    // 3. Λήψη Λεπτομερειών (Get Details)
    public record GetLoanDetailsRequest(
        [Required] Guid LoanId
    );

    // 4. Αρχειοθέτηση/Διαγραφή (Archive)
    public record ArchiveLoanRequest(
        [Required] Guid LoanId,
        string? Reason // Προαιρετικό
    );

    // 5. Λήψη Λίστας (Get List - κενό προς το παρόν, αλλά έτοιμο για φίλτρα)
    public record GetLoansRequest(
    // Μελλοντικά εδώ θα μπουν φίλτρα:
    // int PageNumber,
    // int PageSize,
    // string? StatusFilter
    );

    public record LoanDto(
    Guid Id,
    decimal LoanAmount,
    int TermMonths,
    string Purpose,
    string Status,     
    int StatusCode, 
    DateTime CreatedAt,
    string? ApplicantName
);
    public record DecideLoanRequest(
    Guid LoanId,
    bool Approved,
    string? Comments
);

    // Για το ανέβασμα αρχείου (Ειδική περίπτωση με FormFile)
    public record UploadDocumentRequest(
        Guid LoanId,
        IFormFile File
    );
}
