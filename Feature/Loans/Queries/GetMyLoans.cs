using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Queries
{
    public record LoanDto(Guid Id, decimal Amount, string Status, DateTime CreatedOn);
    public record GetMyLoansQuery : IRequest<List<LoanDto>>;

    public class GetMyLoansQueryHandler : IRequestHandler<GetMyLoansQuery, List<LoanDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyLoansQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<LoanDto>> Handle(GetMyLoansQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            return await _context.LoanApplications
                .Where(x => x.ApplicantId == userId) // Φέρνουμε μόνο τα δικά του
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => new LoanDto(
                    x.Id,
                    x.LoanAmount,
                    x.Status.ToString(), // Μετατροπή Enum σε String
                    x.CreatedOnUtc
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
