namespace CreditFlowAPI.Domain.Entities
{
    public class LoanDocument
    {
        public Guid Id { get; set; }
        public Guid LoanApplicationId { get; set; } 

        public required string FileName { get; set; }
        public required string FilePath { get; set; } 
        public long FileSizeInBytes { get; set; }
        public DateTime UploadedOnUtc { get; set; } = DateTime.UtcNow;
    }
}
