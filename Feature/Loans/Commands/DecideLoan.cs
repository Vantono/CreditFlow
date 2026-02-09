using CreditFlowAPI.Domain.Enums;
using CreditFlowAPI.Domain.Interfaces;
using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Base.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CreditFlowAPI.Feature.Loans.Commands
{


    public record DecideLoanCommand(
        Guid LoanId,
        DecisionType Decision,
        string Comments,
        byte[] RowVersion
    ) : IRequest;

    public class DecideLoanCommandHandler : IRequestHandler<DecideLoanCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DecideLoanCommandHandler> _logger;

        public DecideLoanCommandHandler(
            IApplicationDbContext context,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<DecideLoanCommandHandler> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task Handle(DecideLoanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var loan = await _context.LoanApplications
                    .FirstOrDefaultAsync(x => x.Id == request.LoanId, cancellationToken);

                if (loan == null)
                {
                    throw new KeyNotFoundException($"Loan with id {request.LoanId} not found.");
                }

                var applicant = await _userManager.FindByIdAsync(loan.ApplicantId);
                if (applicant == null)
                {
                    throw new KeyNotFoundException($"Applicant not found");
                }

                var isApproved = request.Decision == DecisionType.Approve;
                loan.Status = isApproved ? LoanStatus.Approved : LoanStatus.Rejected;
                loan.BankerComments = request.Comments;

                await _context.SaveChangesAsync(cancellationToken);

                await _notificationService.SendLoanStatusUpdate(
                    loan.ApplicantId,
                    loan.Id.ToString(),
                    isApproved ? "Approved" : "Rejected",
                    request.Comments
                );

                if (isApproved)
                {
                    var monthlyPayment = CalculateMonthlyPayment(loan.LoanAmount, loan.TermMonths);
                    await _notificationService.SendLoanApprovalEmail(
                        applicant.Email ?? string.Empty,
                        $"{applicant.FirstName} {applicant.LastName}",
                        loan.Id.ToString(),
                        loan.LoanAmount,
                        monthlyPayment
                    );
                }
                else
                {
                    await _notificationService.SendLoanRejectionEmail(
                        applicant.Email ?? string.Empty,
                        $"{applicant.FirstName} {applicant.LastName}",
                        loan.Id.ToString(),
                        request.Comments
                    );
                }

                _logger.LogInformation($"Loan {request.LoanId} decided as {(isApproved ? "APPROVED" : "REJECTED")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing loan decision for {request.LoanId}");
                throw;
            }
        }

        private decimal CalculateMonthlyPayment(decimal principal, int termMonths)
        {
            return Math.Round(principal / termMonths, 2);
        }
    }
}
