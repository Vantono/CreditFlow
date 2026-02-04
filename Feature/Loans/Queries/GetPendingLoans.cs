using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CreditFlowAPI.Base.Identity;

namespace CreditFlowAPI.Feature.Loans.Queries
{
    // DTO: Comprehensive data for banker to review loan applications
    public record PendingLoanDto(
        Guid Id,
        string ApplicantId,
        string ApplicantName,
        string ApplicantEmail,
        decimal Amount,
        int TermMonths,
        string Purpose,
        string EmployerName,
        string JobTitle,
        int YearsEmployed,
        decimal MonthlyIncome,
        decimal MonthlyExpenses,
        decimal InterestRate,
        decimal MonthlyPayment,
        decimal TotalInterest,
        decimal DebtToIncomeRatio,
        string RiskLevel,
        DateTime SubmittedOn,
        byte[] RowVersion
    );

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
                .Where(x => x.Status == Domain.Enums.LoanStatus.Submitted) // Only submitted applications
                .OrderBy(x => x.SubmittedOnUtc) // FIFO (First In First Out)
                .Join(
                    _context.Users,
                    loan => loan.ApplicantId,
                    user => user.Id,
                    (loan, user) => new PendingLoanDto(
                        loan.Id,
                        loan.ApplicantId,
                        $"{user.FirstName} {user.LastName}",
                        user.Email ?? string.Empty,
                        loan.LoanAmount,
                        loan.TermMonths,
                        loan.Purpose,
                        loan.EmployerName ?? string.Empty,
                        loan.JobTitle ?? string.Empty,
                        loan.YearsEmployed,
                        loan.MonthlyIncome,
                        loan.MonthlyExpenses,
                        loan.InterestRate,
                        loan.MonthlyPayment,
                        loan.TotalInterest,
                        loan.DebtToIncomeRatio,
                        loan.RiskLevel ?? "Unknown",
                        loan.SubmittedOnUtc ?? DateTime.MinValue,
                        loan.RowVersion!
                    )
                )
                .ToListAsync(cancellationToken);
        }
    }
}
