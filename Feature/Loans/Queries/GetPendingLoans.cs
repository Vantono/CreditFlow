using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Queries
{
    // DTO: Τι χρειάζεται να βλέπει ο τραπεζικός στη λίστα
    public record PendingLoanDto(Guid Id, string ApplicantId, decimal Amount, DateTime SubmittedOn, byte[] RowVersion);

    public record GetPendingLoansQuery : IRequest<List<PendingLoanDto>>;

    public class GetPendingLoansQueryHandler : IRequestHandler<GetPendingLoansQuery, List<PendingLoanDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPendingLoansQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PendingLoanDto>> Handle(GetPendingLoansQuery request, CancellationToken cancellationToken)
        {
            return await _context.LoanApplications
                .Where(x => x.Status == Domain.Enums.LoanStatus.Submitted) // Μόνο τα υποβληθέντα
                .OrderBy(x => x.SubmittedOnUtc) // FIFO (First In First Out)
                .Select(x => new PendingLoanDto(
                    x.Id,
                    x.ApplicantId, // Εδώ μελλοντικά θα φέρναμε το Όνομα αντί για το ID
                    x.LoanAmount,
                    x.SubmittedOnUtc ?? DateTime.MinValue,
                    x.RowVersion // Στέλνουμε το RowVersion στο Frontend για να το χρησιμοποιήσει μετά στο Decision
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
