using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Commands
{
    public record SubmitLoanCommand(Guid Id) : IRequest; // Δεν επιστρέφει τίποτα, απλά κάνει τη δουλειά

    public class SubmitLoanCommandHandler : IRequestHandler<SubmitLoanCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SubmitLoanCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task Handle(SubmitLoanCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            // Βρίσκουμε την αίτηση
            var entity = await _context.LoanApplications
                .FirstOrDefaultAsync(l => l.Id == request.Id && l.ApplicantId == userId, cancellationToken);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Η αίτηση με ID {request.Id} δεν βρέθηκε.");
            }

            // ΚΑΛΟΥΜΕ ΤΗ LΟΓΙΚΗ ΤΟΥ DOMAIN
            // Αν η κατάσταση δεν είναι Draft, αυτό θα πετάξει Exception αυτόματα
            entity.Submit();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
