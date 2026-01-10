namespace CreditFlowAPI.Domain.Entities
{
    public class LoanDocument
    {
        public Guid Id { get; set; }
        public Guid LoanApplicationId { get; set; } // FK

        public required string FileName { get; set; } // "tax_return_2024.pdf"
        public required string FilePath { get; set; } // "/uploads/loans/guid/tax.pdf"
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedOnUtc { get; set; } = DateTime.UtcNow;
    }
}
