using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Queries
{
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
                .AsNoTracking()
                .Where(x => x.ApplicantId == userId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => new LoanDto(
                    x.Id,                
                    x.LoanAmount,                        
                    x.TermMonths,                        
                    x.Purpose,                           
                    x.Status.ToString(),                 
                    (int)x.Status,                      
                    x.CreatedOnUtc,    
                    x.ApplicantId,
                    x.InterestRate,
                    x.MonthlyPayment,
                    x.TotalInterest,
                    x.DebtToIncomeRatio,
                    x.RiskLevel
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
