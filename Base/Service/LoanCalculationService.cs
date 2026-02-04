namespace CreditFlowAPI.Base.Service
{
    /// <summary>
    /// Loan calculation service for computing payments and interest
    /// </summary>
    public class LoanCalculationService
    {
        /// <summary>
        /// Calculates the monthly payment using standard amortization formula
        /// Formula: M = P * [r(1+r)^n] / [(1+r)^n - 1]
        /// Where: M = Monthly Payment, P = Principal, r = monthly rate, n = number of months
        /// </summary>
        public decimal CalculateMonthlyPayment(decimal principalAmount, decimal annualInterestRate, int termMonths)
        {
            if (principalAmount <= 0 || termMonths <= 0)
                return 0;

            if (annualInterestRate <= 0)
                return principalAmount / termMonths;

            decimal monthlyRate = annualInterestRate / 100 / 12;

            // Standard amortization formula
            decimal numerator = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), termMonths);
            decimal denominator = (decimal)Math.Pow((double)(1 + monthlyRate), termMonths) - 1;

            decimal monthlyPayment = principalAmount * (numerator / denominator);

            return Math.Round(monthlyPayment, 2);
        }

        /// <summary>
        /// Calculates total interest paid over the life of the loan
        /// </summary>
        public decimal CalculateTotalInterest(decimal monthlyPayment, int termMonths, decimal principalAmount)
        {
            decimal totalPaid = monthlyPayment * termMonths;
            decimal totalInterest = totalPaid - principalAmount;

            return Math.Round(Math.Max(totalInterest, 0), 2);
        }

        /// <summary>
        /// Calculates total cost of the loan (principal + interest)
        /// </summary>
        public decimal CalculateTotalCost(decimal monthlyPayment, int termMonths)
        {
            return Math.Round(monthlyPayment * termMonths, 2);
        }

        /// <summary>
        /// Validates that monthly income can support the loan payment
        /// Applies 28% front-end ratio (max 28% of gross income for housing/loan payments)
        /// </summary>
        public bool CanAffordLoan(decimal monthlyPayment, decimal monthlyIncome)
        {
            if (monthlyIncome <= 0)
                return false;

            decimal maxAllowedPayment = monthlyIncome * 0.28m;
            return monthlyPayment <= maxAllowedPayment;
        }
    }
}
