using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Base.Identity;
using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Commands
{
    public record SubmitLoanCommand(Guid Id) : IRequest; // Δεν επιστρέφει τίποτα, απλά κάνει τη δουλειά

    public class SubmitLoanCommandHandler : IRequestHandler<SubmitLoanCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SubmitLoanCommandHandler> _logger;

        public SubmitLoanCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<SubmitLoanCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task Handle(SubmitLoanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId ?? string.Empty;

                // Fetch loan and applicant details
                var entity = await _context.LoanApplications
                    .FirstOrDefaultAsync(l => l.Id == request.Id && l.ApplicantId == userId, cancellationToken);

                if (entity == null)
                {
                    throw new KeyNotFoundException($"Loan application {request.Id} not found.");
                }

                var applicant = await _userManager.FindByIdAsync(userId);
                if (applicant == null)
                {
                    throw new KeyNotFoundException($"Applicant not found");
                }

                // Execute domain logic (Submit sets status to Submitted)
                entity.Submit();

                await _context.SaveChangesAsync(cancellationToken);

                // Send SignalR notification to applicant
                await _notificationService.SendNotificationToUser(
                    userId,
                    "info",
                    "📤 Application Submitted",
                    $"Your loan application for ${entity.LoanAmount:N2} has been submitted successfully and is now under review."
                );

                // Notify bankers about new submission
                await _notificationService.SendBankerNewSubmission(
                    entity.Id.ToString(),
                    $"{applicant.FirstName} {applicant.LastName}",
                    entity.LoanAmount
                );

                // Send email confirmation
                await _notificationService.SendLoanSubmissionConfirmationEmail(
                    applicant.Email ?? string.Empty,
                    $"{applicant.FirstName} {applicant.LastName}",
                    entity.Id.ToString(),
                    entity.LoanAmount
                );

                _logger.LogInformation($"Loan {request.Id} submitted successfully by {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting loan {request.Id}");
                throw;
            }
        }
    }
}
