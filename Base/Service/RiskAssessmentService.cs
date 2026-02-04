namespace CreditFlowAPI.Base.Service
{
    /// <summary>
    /// Simulated credit scoring service for risk assessment
    /// Evaluates loan risk based on income, expenses, and loan amount
    /// </summary>
    public class RiskAssessmentService
    {
        /// <summary>
        /// Calculates the risk level based on debt-to-income ratio
        /// </summary>
        public string CalculateRiskLevel(decimal debtToIncomeRatio)
        {
            // DTI below 20% = Low Risk
            if (debtToIncomeRatio < 20)
                return "Low";

            // DTI between 20% and 43% = Medium Risk
            if (debtToIncomeRatio <= 43)
                return "Medium";

            // DTI above 43% = High Risk
            return "High";
        }

        /// <summary>
        /// Calculates debt-to-income ratio (monthly loan payment vs gross income)
        /// </summary>
        public decimal CalculateDebtToIncomeRatio(decimal monthlyPayment, decimal monthlyIncome)
        {
            if (monthlyIncome <= 0)
                return 100; // High risk if no income

            return (monthlyPayment / monthlyIncome) * 100;
        }

        /// <summary>
        /// Determines interest rate based on risk level and employment history
        /// </summary>
        public decimal DetermineInterestRate(string riskLevel, int yearsEmployed)
        {
            decimal baseRate = riskLevel switch
            {
                "Low" => 4.5m,      // Prime-like rate
                "Medium" => 8.5m,   // Standard rate
                "High" => 12.5m,    // Subprime rate
                _ => 10.0m          // Default
            };

            // Apply employment stability bonus (reduces rate by 0.5% per year, max 2%)
            decimal employmentBonus = Math.Min((yearsEmployed * 0.5m), 2.0m);

            return Math.Max(baseRate - employmentBonus, 3.0m); // Minimum 3% rate
        }
    }
}
