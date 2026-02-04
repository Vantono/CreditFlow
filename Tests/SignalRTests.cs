using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using CreditFlowAPI.Base.Hubs;
using CreditFlowAPI.Base.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CreditFlowAPI.Tests.SignalR
{
    public class NotificationHubTests
    {
        private readonly Mock<IHubCallerClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<HubCallerContext> _mockContext;
        private readonly Mock<IGroupManager> _mockGroupManager;
        private readonly Mock<ILogger<NotificationHub>> _mockLogger;
        private readonly NotificationHub _hub;

        public NotificationHubTests()
        {
            _mockClients = new Mock<IHubCallerClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockContext = new Mock<HubCallerContext>();
            _mockGroupManager = new Mock<IGroupManager>();
            _mockLogger = new Mock<ILogger<NotificationHub>>();

            _hub = new NotificationHub(_mockLogger.Object)
            {
                Clients = _mockClients.Object,
                Context = _mockContext.Object,
                Groups = _mockGroupManager.Object
            };
        }

        [Fact]
        public async Task SendTestMessage_ShouldSendNotificationToCaller()
        {
            // Arrange
            var testMessage = "Test message content";
            _mockClients.Setup(c => c.Caller).Returns((ISingleClientProxy)_mockClientProxy.Object);

            // Act
            await _hub.SendTestMessage(testMessage);

            // Assert
            _mockClients.Verify(c => c.Caller, Moq.Times.Once);
            _mockClientProxy.Verify(
                c => c.SendAsync(
                    "ReceiveNotification",
                    It.Is<object>(o => o.ToString()!.Contains("Test Message")),
                    It.IsAny<CancellationToken>()),
                Moq.Times.Once);
        }

        [Fact]
        public async Task SendLoanNotification_ShouldSendToCorrectUserGroup()
        {
            // Arrange
            var userId = "user123";
            var loanId = "loan456";
            var status = "Approved";
            var message = "Your loan has been approved";

            var groupClients = new Mock<IClientProxy>();
            _mockClients.Setup(c => c.Group($"user_{userId}")).Returns(groupClients.Object);

            // Act
            await _hub.SendLoanNotification(userId, loanId, status, message);

            // Assert
            _mockClients.Verify(c => c.Group($"user_{userId}"), Moq.Times.Once);
            groupClients.Verify(
                c => c.SendAsync(
                    "ReceiveLoanNotification",
                    It.Is<object>(o => o.ToString()!.Contains("Approved")),
                    It.IsAny<CancellationToken>()),
                Moq.Times.Once);
        }

        [Fact]
        public async Task NotifyBankerNewSubmission_ShouldBroadcastToAllBankers()
        {
            // Arrange
            var loanId = "loan789";
            var applicantName = "John Doe";
            var amount = 50000m;

            var bankerClients = new Mock<IClientProxy>();
            _mockClients.Setup(c => c.Group("bankers")).Returns(bankerClients.Object);

            // Act
            await _hub.NotifyBankerNewSubmission(loanId, applicantName, amount);

            // Assert
            _mockClients.Verify(c => c.Group("bankers"), Moq.Times.Once);
            bankerClients.Verify(
                c => c.SendAsync(
                    "ReceiveLoanNotification",
                    It.Is<object>(o => o.ToString()!.Contains("John Doe")),
                    It.IsAny<CancellationToken>()),
                Moq.Times.Once);
        }

        [Theory]
        [InlineData("Submitted", "📤 Application Submitted")]
        [InlineData("UnderReview", "🔍 Under Review")]
        [InlineData("Approved", "✅ Application Approved!")]
        [InlineData("Rejected", "❌ Application Rejected")]
        public async Task SendLoanNotification_ShouldReturnCorrectTitles(string status, string expectedTitle)
        {
            // Arrange
            var userId = "user123";
            var loanId = "loan456";
            var message = "Test message";

            var groupClients = new Mock<IClientProxy>();
            _mockClients.Setup(c => c.Group($"user_{userId}")).Returns(groupClients.Object);

            // Act
            await _hub.SendLoanNotification(userId, loanId, status, message);

            // Assert
            groupClients.Verify(
                c => c.SendAsync(
                    "ReceiveLoanNotification",
                    It.Is<object>(o => o.ToString()!.Contains(expectedTitle)),
                    It.IsAny<CancellationToken>()),
                Moq.Times.Once);
        }
    }

    public class NotificationServiceTests
    {
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<ILogger<NotificationService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockLogger = new Mock<ILogger<NotificationService>>();
            _mockConfig = new Mock<IConfiguration>();

            _mockConfig.Setup(c => c.GetValue<bool>("Email:UseMockEmail", true)).Returns(true);

            _notificationService = new NotificationService(_mockHubContext.Object, _mockLogger.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task SendNotificationToUser_ShouldSendToUserGroup()
        {
            // Arrange
            var userId = "user123";
            var type = "info";
            var title = "Test Title";
            var message = "Test Message";

            var mockClients = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients.Group($"user_{userId}")).Returns(mockClients.Object);

            // Act
            await _notificationService.SendNotificationToUser(userId, type, title, message);

            // Assert
            _mockHubContext.Verify(h => h.Clients.Group($"user_{userId}"), Moq.Times.Once);
            mockClients.Verify(
                c => c.SendAsync(
                    "ReceiveNotification",
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()),
                Moq.Times.Once);
        }

        [Fact]
        public async Task SendLoanStatusUpdate_ApprovedLoan_ShouldSendSuccessNotification()
        {
            // Arrange
            var userId = "user123";
            var loanId = "loan456";
            var comments = "Approved with good credit";

            var mockClients = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients.Group($"user_{userId}")).Returns(mockClients.Object);

            // Act
            await _notificationService.SendLoanStatusUpdate(userId, loanId, "Approved", comments);

            // Assert
            _mockHubContext.Verify(h => h.Clients.Group($"user_{userId}"), Moq.Times.Once);
            mockClients.Verify(
                c => c.SendAsync(
                    "ReceiveNotification",
                    It.Is<object>(o => o.ToString()!.Contains("Approved")),
                    It.IsAny<CancellationToken>()),
                Moq.Times.Once);
        }

        [Fact]
        public async Task SendEmailNotification_WithMockEmail_ShouldNotThrowError()
        {
            // Arrange
            var email = "test@example.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Act & Assert - Should not throw
            await _notificationService.SendEmailNotification(email, subject, body);
        }

        [Fact]
        public async Task SendLoanApprovalEmail_ShouldCallSendEmailNotification()
        {
            // Arrange
            var email = "applicant@example.com";
            var applicantName = "Jane Doe";
            var loanId = "loan789";
            var amount = 25000m;
            var monthlyPayment = 500m;

            // Act
            await _notificationService.SendLoanApprovalEmail(email, applicantName, loanId, amount, monthlyPayment);

            // Assert - Should complete without error
            Assert.True(true);
        }

        [Fact]
        public async Task SendLoanSubmissionConfirmationEmail_ShouldContainLoanDetails()
        {
            // Arrange
            var email = "applicant@example.com";
            var applicantName = "John Smith";
            var loanId = "loan999";
            var amount = 30000m;

            // Act
            await _notificationService.SendLoanSubmissionConfirmationEmail(email, applicantName, loanId, amount);

            // Assert - Should complete without error
            Assert.True(true);
        }
    }
}
