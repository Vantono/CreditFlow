using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace CreditFlowAPI.Feature.Loans.Commands
{
    public record CreateLoanCommand(decimal LoanAmount, int TermMonths, string Purpose) : IRequest<Guid>;
    public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
    {
        public CreateLoanCommandValidator()
        {
            RuleFor(v => v.LoanAmount)
                .GreaterThan(0).WithMessage("Το ποσό πρέπει να είναι θετικό.")
                .LessThan(500000).WithMessage("Το μέγιστο ποσό είναι 500,000.");

            RuleFor(v => v.TermMonths)
                .InclusiveBetween(6, 120).WithMessage("Η διάρκεια πρέπει να είναι από 6 έως 120 μήνες.");

            RuleFor(v => v.Purpose)
                .NotEmpty().WithMessage("Ο σκοπός δανείου είναι υποχρεωτικός.")
                .MaximumLength(200);
        }
    }

    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateLoanCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            // Παίρνουμε το ID του χρήστη από το Token
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

            var entity = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicantId = userId,
                LoanAmount = request.LoanAmount,
                TermMonths = request.TermMonths,
                Purpose = request.Purpose,
                Status = Domain.Enums.LoanStatus.Draft,
                RowVersion = Guid.NewGuid().ToByteArray()
            };

            _context.LoanApplications.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
