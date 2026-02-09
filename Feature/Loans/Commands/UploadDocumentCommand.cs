using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Commands
{
    public record UploadDocumentCommand(Guid LoanId, IFormFile File) : IRequest<Guid>;

    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;
        private readonly ILogger<UploadDocumentCommandHandler> _logger;

        public UploadDocumentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IFileService fileService,
            ILogger<UploadDocumentCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var loan = await _context.LoanApplications
                .Include(l => l.Documents)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId && l.ApplicantId == userId, cancellationToken);

            if (loan == null)
                throw new KeyNotFoundException($"Loan {request.LoanId} not found");

            if (loan.Status != Domain.Enums.LoanStatus.Draft)
            {
                throw new InvalidOperationException("Cannot upload documents to a submitted loan application");
            }

            // Validate file before saving
            if (!_fileService.IsValidDocumentType(request.File))
                throw new InvalidOperationException($"File type not allowed");

            if (!_fileService.IsValidFileSize(request.File))
                throw new InvalidOperationException($"File size exceeds maximum allowed limit");

            try
            {
                var filePath = await _fileService.SaveLoanDocumentAsync(request.File, request.LoanId, cancellationToken);

                var document = new LoanDocument
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = loan.Id,
                    FileName = request.File.FileName,
                    FilePath = filePath,
                    FileSizeInBytes = request.File.Length,
                    UploadedOnUtc = DateTime.UtcNow
                };

                _context.LoanDocuments.Add(document);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Document uploaded successfully for loan {request.LoanId}: {request.File.FileName}");

                return document.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upload document for loan {request.LoanId}");
                throw;
            }
        }
    }
}
