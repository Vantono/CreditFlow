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

            var loan = await _context.LoanApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            // 2. Έλεγχος: Υπάρχει;
            if (loan == null)
            {
                throw new KeyNotFoundException($"Loan with id {request.Id} was not found.");
            }

            if (loan.ApplicantId != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this loan.");
            }

            return new LoanDto(
                loan.Id,
                loan.LoanAmount,
                loan.TermMonths,
                loan.Purpose,
                loan.Status.ToString(),
                (int)loan.Status,
                loan.CreatedOnUtc,
                "Me", 
                loan.InterestRate,
                loan.MonthlyPayment,
                loan.TotalInterest,
                loan.DebtToIncomeRatio,
                loan.RiskLevel
            );
        }
    }
}
