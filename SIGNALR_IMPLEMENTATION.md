# CreditFlow SignalR Real-Time Implementation

## Overview

This document describes the complete SignalR real-time messaging implementation for the CreditFlow loan management system. The system provides real-time notifications for loan status updates, email confirmations, and document uploads.

## Architecture

### Backend Components

#### 1. **NotificationHub** ([Base/Hubs/NotificationHub.cs](Base/Hubs/NotificationHub.cs))
- **Purpose**: SignalR hub for real-time bidirectional communication
- **Key Features**:
  - User-based group routing (connects users to personal groups: `user_{userId}`)
  - Banker dashboard notifications
  - Status-based notification formatting with emoji indicators
  - Automatic connection management with JWT authentication

**Key Methods**:
```csharp
public async Task SendLoanNotification(string userId, string loanId, string status, string message)
public async Task NotifyBankerNewSubmission(LoanApplication loan)
private string GetLoanStatusTitle(string status) // Returns emoji-formatted status
```

#### 2. **NotificationService** ([Base/Service/NotificationService.cs](Base/Service/NotificationService.cs))
- **Purpose**: Centralized notification management for email and SignalR messaging
- **Features**:
  - Dual-mode email system (mock for development, SMTP for production)
  - HTML-formatted email templates
  - Coordinated email + SignalR notification delivery

**Key Methods**:
```csharp
public async Task SendLoanApprovalEmail(string email, LoanApplication loan)
public async Task SendLoanRejectionEmail(string email, LoanApplication loan)
public async Task SendLoanSubmissionConfirmationEmail(string email, LoanApplication loan)
```

#### 3. **FileService** ([Base/Service/FileService.cs](Base/Service/FileService.cs))
- **Purpose**: Enterprise-grade document management
- **Features**:
  - 5MB file size limit
  - Whitelist validation (.pdf, .doc, .docx, .xls, .xlsx, .jpg, .jpeg, .png)
  - Organized storage in `uploads/loans/{loanId}/`
  - Unique filename generation with Guid

**Key Methods**:
```csharp
public async Task<LoanDocument> SaveLoanDocumentAsync(int loanId, IFormFile file)
public async Task DeleteLoanDocumentAsync(int documentId)
public bool IsValidDocumentType(string fileName)
public bool IsValidFileSize(long fileSize)
```

### CQRS Command Integration

#### **SubmitLoanCommand** ([Feature/Loans/Commands/SubmitLoanCommand.cs](Feature/Loans/Commands/SubmitLoanCommand.cs))
- Triggers loan submission workflow
- Sends confirmation email to applicant
- Notifies banker dashboard via SignalR
- Logs audit trail

#### **DecideLoanCommand** ([Feature/Loans/Commands/DecideLoan.cs](Feature/Loans/Commands/DecideLoan.cs))
- Executes approval/rejection decision
- Sends decision-specific emails (approval or rejection template)
- Notifies applicant via SignalR with detailed status
- Logs audit trail

## Frontend Components

### 1. **SignalRService** ([Angular/credit-flow-web/src/app/core/services/signalr.service.ts](Angular/credit-flow-web/src/app/core/services/signalr.service.ts))

A robust Angular service wrapper around @microsoft/signalr library with:
- Lazy initialization pattern (prevents module load errors)
- Automatic reconnection with exponential backoff (0ms, 2s, 10s, 30s)
- JWT token-based authentication
- Typed notification handling
- Notification history (last 50 notifications)

**Key Signals**:
```typescript
notifications = signal<LoanNotification[]>([])
isConnected = signal(false)
lastNotification = signal<LoanNotification | null>(null)
```

**Event Handlers**:
- `ReceiveNotification`: Generic notification handler
- `ReceiveLoanNotification`: Loan-specific status updates
- Automatic connection state management

### 2. **NotificationPanelComponent** ([Angular/credit-flow-web/src/app/core/components/notification-panel.component.ts](Angular/credit-flow-web/src/app/core/components/notification-panel.component.ts))

Professional notification center with:
- Floating notification icon with unread badge
- Slide-in/out panel animation
- Notification grouping and filtering
- Mark as read functionality
- Clear all notifications
- Responsive design (mobile-friendly)
- Time-based formatting (e.g., "5m ago", "2h ago")

**Features**:
- Real-time unread count
- Type-based emoji indicators
- Persistent notification history
- Clean, modern UI with smooth animations

### 3. **RealtimeDashboardComponent** ([Angular/credit-flow-web/src/app/core/components/realtime-dashboard.component.ts](Angular/credit-flow-web/src/app/core/components/realtime-dashboard.component.ts))

Comprehensive loan management dashboard featuring:
- Real-time connection status indicator
- Quick stats (total, approved, pending, total approved amount)
- Loan application cards with status indicators
- Progress tracking
- Action buttons (Submit, Approve, Reject)
- Recent activity feed
- Responsive grid layout

**Key Computed Signals**:
```typescript
approvedCount = computed(() => /* filter approved loans */)
pendingCount = computed(() => /* filter pending loans */)
totalApproved = computed(() => /* sum approved amounts */)
recentActivity = computed(() => /* map notifications to activity */)
```

### 4. **NotificationToastComponent** ([Angular/credit-flow-web/src/app/core/components/notification-toast.component.ts](Angular/credit-flow-web/src/app/core/components/notification-toast.component.ts))

Toast notification system with:
- Auto-dismiss after configurable duration (default 5s)
- Type-based styling (success, error, warn, info)
- Smooth slide-in/out animations
- Manual close button
- Multiple toast stacking
- Mobile-responsive positioning

## Configuration

### Backend Configuration ([appsettings.json](appsettings.json))

```json
{
  "Email": {
    "UseMockEmail": true,
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "creditflow@example.com",
    "FromPassword": "your_app_password"
  },
  "FileUpload": {
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"],
    "UploadPath": "uploads"
  }
}
```

### Frontend Configuration

**SignalR Hub URL**: Configured in `SignalRService.ts`
- Default: `/hubs/notifications`
- Production: Update to match your backend URL (e.g., `https://api.creditflow.com/hubs/notifications`)

**Authentication**: JWT token from localStorage
- Token key: `token`
- Passed via `accessTokenFactory` in HubConnectionBuilder

## Installation & Setup

### Backend Setup

1. **Add NuGet Packages** (if needed):
   ```bash
   dotnet add package Microsoft.AspNetCore.SignalR
   dotnet add package Microsoft.AspNetCore.SignalR.Client
   ```

2. **Register Services** in [Program.cs](Program.cs):
   ```csharp
   builder.Services.AddScoped<INotificationService, NotificationService>();
   builder.Services.AddScoped<IFileService, FileService>();
   
   builder.Services.AddSignalR();
   
   // In app mapping:
   app.MapHub<NotificationHub>("/notificationHub");
   ```

### Frontend Setup

1. **Install @microsoft/signalr**:
   ```bash
   cd Angular/credit-flow-web
   npm install @microsoft/signalr@8.0.0
   ```

2. **Add Components to App** (in root component or layout):
   ```typescript
   import { NotificationPanelComponent } from './core/components/notification-panel.component';
   import { NotificationToastComponent } from './core/components/notification-toast.component';
   import { RealtimeDashboardComponent } from './core/components/realtime-dashboard.component';

   @Component({
     imports: [
       NotificationPanelComponent,
       NotificationToastComponent,
       RealtimeDashboardComponent
     ]
   })
   ```

3. **Add to Template**:
   ```html
   <!-- In your root/layout component -->
   <app-notification-panel></app-notification-panel>
   <app-notification-toast></app-notification-toast>
   ```

## Data Flow

### Loan Submission Flow

```
1. User submits loan application
   ↓
2. SubmitLoanCommand handler executes:
   - Updates loan status to "Submitted"
   - Calls NotificationService.SendLoanSubmissionConfirmationEmail()
   - Email sent via SMTP/mock
   - SignalR notification sent via NotificationHub.SendLoanNotification()
   ↓
3. SignalR message routed to:
   - Applicant: user_{applicantId} group
   - Bankers: bankers group
   ↓
4. Frontend receives notification:
   - NotificationService captures event
   - Toast component displays notification
   - Notification panel shows in history
   - Dashboard updates activity feed
```

### Loan Decision Flow

```
1. Banker decides on loan (approve/reject)
   ↓
2. DecideLoanCommand handler executes:
   - Updates loan status to "Approved" or "Rejected"
   - Sends decision-specific email
   - Creates SignalR notification with decision details
   ↓
3. Notifications sent:
   - Email to applicant (approved/rejected template)
   - SignalR to applicant's personal group
   ↓
4. Frontend displays:
   - Toast notification with decision
   - Notification panel entry
   - Dashboard status update
```

## Security

### Authentication
- **JWT Tokens**: Required for all SignalR connections
- **Token Extraction**: From Authorization header or query string
- **Validation**: Performed in NotificationHub.OnMessageReceived

### Authorization
- **User-based Routing**: Users only receive notifications in their personal groups
- **Banker Dashboard**: Separate group for banker notifications
- **Audit Logging**: All decisions logged with user context

### File Upload Security
- **Whitelist Validation**: Only allowed file types
- **Size Limits**: 5MB maximum per file
- **Organized Storage**: Files stored in loan-specific directories
- **Unique Names**: Guid-based to prevent collisions

## Testing

### Unit Tests ([Tests/SignalRTests.cs](Tests/SignalRTests.cs))

Comprehensive test suite with 12+ test cases:
```csharp
[Fact]
public async Task SendTestMessage_WithValidMessage_SendsSuccessfully()

[Fact]
public async Task SendLoanNotification_WithValidLoan_SendsToCorrectUser()

[Fact]
public async Task NotifyBankerNewSubmission_WithValidLoan_BroadcastsToBankersGroup()
```

**Running Tests**:
```bash
dotnet test
```

## Performance Optimization

### SignalR Configuration
- **Automatic Reconnect**: Exponential backoff to reduce server load
- **Message Filtering**: Server-side group routing prevents unnecessary broadcasts
- **Connection Pooling**: ASP.NET Core manages connection lifecycle

### Frontend Optimization
- **Lazy Initialization**: SignalRService only initializes on demand
- **Signal Efficiency**: Angular signals update only affected components
- **Notification Limits**: Maintains last 50 notifications only
- **Computed Properties**: Avoid unnecessary recalculation

## Troubleshooting

### Connection Issues

**Problem**: "SignalR not connected" error
- **Solution**: Ensure JWT token is in localStorage under key `token`
- **Solution**: Verify backend is running and hub URL is correct

**Problem**: "Hub URL not found" (404)
- **Solution**: Verify hub mapping in Program.cs: `app.MapHub<NotificationHub>("/notificationHub");`
- **Solution**: Check frontend URL matches backend (protocol, host, port, path)

### Email Issues

**Problem**: Emails not sending
- **Solution**: If `UseMockEmail: true`, check browser console for mock email logs
- **Solution**: If `UseMockEmail: false`, verify SMTP settings in appsettings.json
- **Solution**: Check application logs for SMTP errors

### File Upload Issues

**Problem**: File upload fails with "File type not allowed"
- **Solution**: Check `AllowedExtensions` in appsettings.json
- **Solution**: Verify file size is under `MaxFileSize` (5MB)

### Notification Not Appearing

**Problem**: Notification sent but not displayed
- **Solution**: Verify SignalRService.connect() is called on app init
- **Solution**: Check browser console for JavaScript errors
- **Solution**: Verify notification panel/toast components are in template

## Future Enhancements

- [ ] WebSocket fallback for older browsers
- [ ] Notification persistence (database storage)
- [ ] Advanced filtering and search
- [ ] Notification scheduling and templates
- [ ] Multi-language support
- [ ] SMS notifications integration
- [ ] Push notifications support

## References

- [ASP.NET Core SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr)
- [@microsoft/signalr NPM Package](https://www.npmjs.com/package/@microsoft/signalr)
- [Angular Signals Documentation](https://angular.io/guide/signals)
- [Angular Animations](https://angular.io/guide/animations)

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review the test cases for implementation examples
3. Check browser and server logs for error details
4. Contact the development team

---

**Last Updated**: January 2025
**Version**: 1.0.0
