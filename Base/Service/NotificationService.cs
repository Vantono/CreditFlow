using CreditFlowAPI.Base.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Net.Mail;

namespace CreditFlowAPI.Base.Service
{
    public interface INotificationService
    {
        Task SendNotificationToUser(string userId, string type, string title, string message);
        Task SendLoanStatusUpdate(string userId, string loanId, string status, string comments);
        Task SendEmailNotification(string email, string subject, string body, bool isHtml = false);
        Task SendLoanApprovalEmail(string email, string applicantName, string loanId, decimal amount, decimal monthlyPayment);
        Task SendLoanRejectionEmail(string email, string applicantName, string loanId, string reason);
        Task SendLoanSubmissionConfirmationEmail(string email, string applicantName, string loanId, decimal amount);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _config;

        public NotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger,
            IConfiguration config)
        {
            _hubContext = hubContext;
            _logger = logger;
            _config = config;
        }

        public async Task SendNotificationToUser(string userId, string type, string title, string message)
        {
            try
            {
                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        Type = type,
                        Title = title,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation($"Notification sent to user {userId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to user {userId}");
            }
        }

        public async Task SendLoanStatusUpdate(string userId, string loanId, string status, string comments)
        {
            var title = status switch
            {
                "Approved" => "✅ Loan Application Approved!",
                "Rejected" => "❌ Loan Application Rejected",
                _ => "Loan Status Updated"
            };

            var message = status switch
            {
                "Approved" => $"Great news! Your loan application has been approved. {comments}",
                "Rejected" => $"Unfortunately, your loan application has been rejected. {comments}",
                _ => $"Your loan status has been updated to {status}."
            };

            await SendNotificationToUser(userId,
                status == "Approved" ? "success" : "warn",
                title,
                message);
        }

        public async Task SendEmailNotification(string email, string subject, string body, bool isHtml = false)
        {
            try
            {
                // Check if we're in development mode - use mock email
                var isDevelopment = _config.GetValue<bool>("Email:UseMockEmail", true);

                if (isDevelopment)
                {
                    _logger.LogInformation($"[MOCK EMAIL] To: {email}, Subject: {subject}");
                    _logger.LogInformation($"[MOCK EMAIL] Body: {body}");
                    return;
                }

                // Real email sending (for production)
                var smtpServer = _config["Email:SmtpServer"];
                var smtpPort = _config.GetValue<int>("Email:SmtpPort", 587);
                var smtpUser = _config["Email:SmtpUser"];
                var smtpPassword = _config["Email:SmtpPassword"];
                var fromEmail = _config["Email:FromEmail"] ?? "noreply@creditflow.com";

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                    var mailMessage = new MailMessage(fromEmail ?? "noreply@creditflow.com", email)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml
                    };

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent to {email}: {subject}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
            }
        }

        public async Task SendLoanApprovalEmail(string email, string applicantName, string loanId, decimal amount, decimal monthlyPayment)
        {
            var subject = "🎉 Your Loan Application Has Been Approved!";
            var body = $@"
                <h2>Loan Application Approved</h2>
                <p>Dear {applicantName},</p>
                <p>Great news! Your loan application has been approved.</p>
                <div style='border: 1px solid #ddd; padding: 15px; margin: 15px 0;'>
                    <p><strong>Loan ID:</strong> {loanId}</p>
                    <p><strong>Loan Amount:</strong> ${amount:N2}</p>
                    <p><strong>Monthly Payment:</strong> ${monthlyPayment:N2}</p>
                </div>
                <p>Please contact us at support@creditflow.com if you have any questions.</p>
                <p>Best regards,<br/>CreditFlow Team</p>";

            await SendEmailNotification(email, subject, body, true);
        }

        public async Task SendLoanRejectionEmail(string email, string applicantName, string loanId, string reason)
        {
            var subject = "Loan Application Status Update";
            var body = $@"
                <h2>Loan Application Status</h2>
                <p>Dear {applicantName},</p>
                <p>Thank you for applying with CreditFlow. After careful review of your application, we regret to inform you that your loan request has been declined at this time.</p>
                <div style='border: 1px solid #ddd; padding: 15px; margin: 15px 0;'>
                    <p><strong>Loan ID:</strong> {loanId}</p>
                    <p><strong>Reason:</strong> {reason}</p>
                </div>
                <p>We encourage you to reapply in the future or contact us to discuss other options at support@creditflow.com</p>
                <p>Best regards,<br/>CreditFlow Team</p>";

            await SendEmailNotification(email, subject, body, true);
        }

        public async Task SendLoanSubmissionConfirmationEmail(string email, string applicantName, string loanId, decimal amount)
        {
            var subject = "📤 Loan Application Submitted Successfully";
            var body = $@"
                <h2>Loan Application Submitted</h2>
                <p>Dear {applicantName},</p>
                <p>Your loan application has been successfully submitted and is now under review.</p>
                <div style='border: 1px solid #ddd; padding: 15px; margin: 15px 0;'>
                    <p><strong>Loan ID:</strong> {loanId}</p>
                    <p><strong>Loan Amount:</strong> ${amount:N2}</p>
                    <p><strong>Status:</strong> Under Review</p>
                </div>
                <p>You will receive email notifications as your application progresses through our review process.</p>
                <p>Thank you for choosing CreditFlow!<br/>CreditFlow Team</p>";

            await SendEmailNotification(email, subject, body, true);
        }
    }
}
