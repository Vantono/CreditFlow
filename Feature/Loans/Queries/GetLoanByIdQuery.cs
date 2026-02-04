using CreditFlowAPI.Base.Persistance;
using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Queries
{
    public record GetLoanByIdQuery(Guid Id) : IRequest<LoanDto>;

    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanDto>
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetLoanByIdQueryHandler(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<LoanDto> Handle(GetLoanByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            // 1. Ψάχνουμε το δάνειο στη βάση με βάση το ID
            var loan = await _context.LoanApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // 2. Έλεγχος: Υπάρχει;
            if (loan == null)
            {
                throw new KeyNotFoundException($"Loan with id {request.Id} was not found.");
            }

            // 3. Security Check: Ανήκει στον χρήστη που το ζήτησε;
            // (Αν θέλεις να μπορεί να το δει και ο Banker, θα χρειαστείς επιπλέον λογική εδώ)
            // Προς το παρόν το αφήνουμε απλό: Αν δεν είναι δικό του, error.
            if (loan.ApplicantId != userId)
            {
                // Μπορείς να πετάξεις UnauthorizedAccessException ή απλά να πεις ότι δεν βρέθηκε για ασφάλεια
                throw new UnauthorizedAccessException("You don't have permission to view this loan.");
            }

            // 4. Mapping σε DTO (Προσοχή στη σειρά των παραμέτρων του Record!)
            return new LoanDto(
                loan.Id,
                loan.LoanAmount,
                loan.TermMonths,
                loan.Purpose,
                loan.Status.ToString(),
                (int)loan.Status,
                loan.CreatedOnUtc,
                "Me", // Ή loan.ApplicantId αν θες το ID, καθώς δεν έχουμε ακόμα το Join με το όνομα
                loan.InterestRate,
                loan.MonthlyPayment,
                loan.TotalInterest,
                loan.DebtToIncomeRatio,
                loan.RiskLevel
            );
        }
    }
}
