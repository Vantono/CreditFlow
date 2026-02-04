using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Domain.Interfaces;
using CreditFlowAPI.Base.Service;
using FluentValidation;
using MediatR;

namespace CreditFlowAPI.Feature.Loans.Commands
{
    public record CreateLoanCommand(
        decimal LoanAmount,
        int TermMonths,
        string Purpose,
        string EmployerName,
        string JobTitle,
        int YearsEmployed,
        decimal MonthlyIncome,
        decimal MonthlyExpenses
    ) : IRequest<Guid>;

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

            RuleFor(v => v.EmployerName)
                .NotEmpty().WithMessage("Το όνομα του εργοδότη είναι υποχρεωτικό.")
                .MaximumLength(200);

            RuleFor(v => v.JobTitle)
                .NotEmpty().WithMessage("Ο τίτλος εργασίας είναι υποχρεωτικός.")
                .MaximumLength(100);

            RuleFor(v => v.MonthlyIncome)
                .GreaterThan(0).WithMessage("Το μηνιαίο εισόδημα πρέπει να είναι θετικό.");

            RuleFor(v => v.MonthlyExpenses)
                .GreaterThanOrEqualTo(0).WithMessage("Τα μηνιαία έξοδα πρέπει να είναι >= 0.");
        }
    }

    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly RiskAssessmentService _riskAssessmentService;
        private readonly LoanCalculationService _loanCalculationService;

        public CreateLoanCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            RiskAssessmentService riskAssessmentService,
            LoanCalculationService loanCalculationService
        )
        {
            _context = context;
            _currentUserService = currentUserService;
            _riskAssessmentService = riskAssessmentService;
            _loanCalculationService = loanCalculationService;
        }

        public async Task<Guid> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            // Παίρνουμε το ID του χρήστη από το Token
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

            // Calculate monthly payment
            var monthlyPayment = _loanCalculationService.CalculateMonthlyPayment(
                request.LoanAmount,
                8.5m,  // Default rate, will be calculated below
                request.TermMonths
            );

            // Calculate debt-to-income ratio
            var debtToIncomeRatio = _riskAssessmentService.CalculateDebtToIncomeRatio(
                monthlyPayment,
                request.MonthlyIncome
            );

            // Determine risk level
            var riskLevel = _riskAssessmentService.CalculateRiskLevel(debtToIncomeRatio);

            // Determine interest rate based on risk level and employment history
            var interestRate = _riskAssessmentService.DetermineInterestRate(riskLevel, request.YearsEmployed);

            // Recalculate monthly payment with actual interest rate
            monthlyPayment = _loanCalculationService.CalculateMonthlyPayment(
                request.LoanAmount,
                interestRate,
                request.TermMonths
            );

            // Recalculate DTI with actual payment
            debtToIncomeRatio = _riskAssessmentService.CalculateDebtToIncomeRatio(
                monthlyPayment,
                request.MonthlyIncome
            );

            // Calculate total interest
            var totalInterest = _loanCalculationService.CalculateTotalInterest(
                monthlyPayment,
                request.TermMonths,
                request.LoanAmount
            );

            var entity = new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicantId = userId,
                LoanAmount = request.LoanAmount,
                TermMonths = request.TermMonths,
                Purpose = request.Purpose,
                EmployerName = request.EmployerName,
                JobTitle = request.JobTitle,
                YearsEmployed = request.YearsEmployed,
                MonthlyIncome = request.MonthlyIncome,
                MonthlyExpenses = request.MonthlyExpenses,
                InterestRate = interestRate,
                MonthlyPayment = monthlyPayment,
                TotalInterest = totalInterest,
                DebtToIncomeRatio = debtToIncomeRatio,
                RiskLevel = riskLevel,
                Status = Domain.Enums.LoanStatus.Draft,
                RowVersion = Guid.NewGuid().ToByteArray()
            };

            _context.LoanApplications.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
