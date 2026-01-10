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
        // Θα χρειαστείς ένα FileService για να σώσεις το αρχείο φυσικά στον δίσκο
        // private readonly IFileService _fileService; 

        public UploadDocumentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var loan = await _context.LoanApplications
                .Include(l => l.Documents)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId && l.ApplicantId == userId, cancellationToken);

            if (loan == null) throw new KeyNotFoundException("Loan not found");

            // VALIDATION: Δεν ανεβάζουμε αρχεία αν έχει γίνει Submit!
            if (loan.Status != Domain.Enums.LoanStatus.Draft)
            {
                throw new InvalidOperationException("Δεν μπορείτε να ανεβάσετε έγγραφα σε υποβληθείσα αίτηση.");
            }

            // Εδώ θα καλούσες το FileService για save στον δίσκο
            // var filePath = await _fileService.SaveFileAsync(request.File);
            var fakePath = $"/uploads/{Guid.NewGuid()}.pdf"; // Mock για τώρα

            var document = new LoanDocument
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = loan.Id,
                FileName = request.File.FileName,
                FilePath = fakePath,
                FileSizeInBytes = request.File.Length
            };

            _context.LoanDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return document.Id;
        }
    }
}
