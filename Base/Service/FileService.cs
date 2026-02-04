using Microsoft.AspNetCore.Http;

namespace CreditFlowAPI.Base.Service
{
    public interface IFileService
    {
        Task<string> SaveLoanDocumentAsync(IFormFile file, Guid loanId, CancellationToken cancellationToken);
        Task DeleteLoanDocumentAsync(string filePath, CancellationToken cancellationToken);
        Task<byte[]> GetLoanDocumentAsync(string filePath, CancellationToken cancellationToken);
        bool IsValidDocumentType(IFormFile file);
        bool IsValidFileSize(IFormFile file);
    }

    public class FileService : IFileService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadsBasePath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedExtensions;

        public FileService(IConfiguration config, ILogger<FileService> logger)
        {
            _config = config;
            _logger = logger;
            _uploadsBasePath = _config["FileUpload:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            _maxFileSize = _config.GetValue<long>("FileUpload:MaxFileSizeInBytes", 5 * 1024 * 1024); // 5MB default
            _allowedExtensions = _config.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
                ?? new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
        }

        public async Task<string> SaveLoanDocumentAsync(IFormFile file, Guid loanId, CancellationToken cancellationToken)
        {
            try
            {
                // Validation
                if (!IsValidDocumentType(file))
                    throw new InvalidOperationException($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

                if (!IsValidFileSize(file))
                    throw new InvalidOperationException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");

                // Create loan-specific directory
                var loanUploadPath = Path.Combine(_uploadsBasePath, "loans", loanId.ToString());
                Directory.CreateDirectory(loanUploadPath);

                // Generate unique filename to prevent conflicts
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";
                var fullFilePath = Path.Combine(loanUploadPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine("loans", loanId.ToString(), uniqueFileName);
                _logger.LogInformation($"Document saved successfully: {relativePath}");

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save document for loan {loanId}");
                throw;
            }
        }

        public async Task DeleteLoanDocumentAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = Path.Combine(_uploadsBasePath, filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Document deleted: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"Document not found for deletion: {filePath}");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete document: {filePath}");
                throw;
            }
        }

        public async Task<byte[]> GetLoanDocumentAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = Path.Combine(_uploadsBasePath, filePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException($"Document not found: {filePath}");

                var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
                _logger.LogInformation($"Document retrieved: {filePath}");

                return fileBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve document: {filePath}");
                throw;
            }
        }

        public bool IsValidDocumentType(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            return _allowedExtensions.Contains(fileExtension);
        }

        public bool IsValidFileSize(IFormFile file)
        {
            return file.Length <= _maxFileSize;
        }
    }
}
