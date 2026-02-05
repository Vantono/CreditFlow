using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CreditFlowAPI.Base.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogInformation($"User {userId} connected with connection ID {Context.ConnectionId}");
            }

            if (Context.User?.IsInRole("Banker") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "bankers");
                _logger.LogInformation($"Banker {userId} added to bankers group");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client can send a test message
        public async Task SendTestMessage(string message)
        {
            var userId = Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            _logger.LogInformation($"Test message from {userId}: {message}");

            await Clients.Caller.SendAsync("ReceiveNotification", new
            {
                Type = "info",
                Title = "Test Message",
                Message = $"Echo: {message}",
                Timestamp = DateTime.UtcNow
            });
        }

        // Send loan status notification to applicant
        public async Task SendLoanNotification(string userId, string loanId, string status, string message, object? additionalData = null)
        {
            try
            {
                var notification = new
                {
                    Type = "loan_status",
                    LoanId = loanId,
                    Status = status,
                    Title = GetLoanStatusTitle(status),
                    Message = message,
                    AdditionalData = additionalData,
                    Timestamp = DateTime.UtcNow
                };

                await Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveLoanNotification", notification);

                _logger.LogInformation($"Loan notification sent to user {userId}: {status} for loan {loanId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send loan notification to user {userId}");
            }
        }

        // Broadcast to all bankers about new loan submission
        public async Task NotifyBankerNewSubmission(string loanId, string applicantName, decimal amount)
        {
            try
            {
                var notification = new
                {
                    Type = "new_loan_submission",
                    LoanId = loanId,
                    ApplicantName = applicantName,
                    Amount = amount,
                    Title = "📌 New Loan Application",
                    Message = $"New loan application from {applicantName} for ${amount:N2}",
                    Timestamp = DateTime.UtcNow
                };

                await Clients.Group("bankers")
                    .SendAsync("ReceiveLoanNotification", notification);

                _logger.LogInformation($"Banker notification sent for new loan {loanId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send banker notification for loan {loanId}");
            }
        }

        private string GetLoanStatusTitle(string status) => status switch
        {
            "Submitted" => "📤 Application Submitted",
            "UnderReview" => "🔍 Under Review",
            "Approved" => "✅ Application Approved!",
            "Rejected" => "❌ Application Rejected",
            _ => "📌 Loan Status Update"
        };
    }
}
