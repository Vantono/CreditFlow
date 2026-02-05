# CreditFlow | Enterprise Loan Management System

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.7-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![PrimeNG](https://img.shields.io/badge/PrimeNG-18-007ACC?logo=prime)](https://primeng.org/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-00A8E1)](https://dotnet.microsoft.com/apps/aspnet/signalr)

**CreditFlow** is a production-grade, full-stack enterprise application engineered to streamline the end-to-end loan application and approval lifecycle. Built with modern architectural patterns and cutting-edge technologies, it provides secure, real-time interaction between loan applicants and financial institution personnel.

---

## 🎯 Project Overview

The system addresses the complete loan lifecycle through specialized functional modules:

### Core Capabilities
- **Identity & Access Management** - Secure user authentication with Role-Based Access Control (RBAC)
- **Customer Portal** - Self-service interface for loan applications with real-time status tracking
- **Banker Console** - Administrative dashboard for reviewing and deciding loan applications
- **Real-time Notifications** - Instant updates via SignalR for status changes and submissions
- **Document Management** - Secure file upload with type/size validation
- **Audit Trail** - Comprehensive logging of all system actions for compliance

---

## 🏗️ Technical Architecture

### Backend Stack (.NET 10)

#### Architectural Patterns
- **Clean Architecture** - Separation of concerns with clearly defined layers
- **CQRS** (Command Query Responsibility Segregation) via **MediatR**
- **Domain-Driven Design** - Rich domain models with encapsulated business logic
- **Vertical Slice Architecture** - Feature folders for cohesive functionality

#### Core Technologies
| Technology | Purpose | Version |
|------------|---------|---------|
| **ASP.NET Core** | Web API Framework | 10.0 |
| **Entity Framework Core** | ORM & Database Management | 10.0 |
| **MediatR** | CQRS Implementation | 12.x |
| **ASP.NET Identity** | User Management & Authentication | 10.0 |
| **SignalR** | Real-time Bidirectional Communication | 10.0 |
| **FluentValidation** | Server-side Validation | 11.x |
| **SQL Server** | Relational Database | 2022 |

#### Security Implementation
- **JWT Bearer Authentication** - Stateless token-based auth with configurable expiration
- **Role-Based Authorization** - Granular access control (Customer/Banker roles)
- **Password Hashing** - ASP.NET Identity with PBKDF2
- **HTTP-only Cookies** - Secure token storage (optional configuration)
- **CORS Policy** - Configurable cross-origin resource sharing

#### Backend Services
```csharp
✓ NotificationService     // Real-time SignalR + Email notifications
✓ FileService             // Document upload with validation
✓ AuditService            // Comprehensive action logging
✓ CurrentUserService      // User context resolution
✓ TokenService            // JWT generation & management
✓ LoanCalculationService  // Interest & payment calculations
✓ RiskAssessmentService   // Credit risk evaluation
```

---

### Frontend Stack (Angular 21)

#### Modern Angular Features
- **Standalone Components** - No NgModules, simplified architecture
- **Angular Signals** - Fine-grained reactivity without zone.js overhead
- **Typed Reactive Forms** - Full type safety in form controls
- **Functional Route Guards** - Modern guard implementation
- **Signal-based State Management** - Custom stores eliminating NGRX complexity

#### Core Technologies
| Technology | Purpose | Version |
|------------|---------|---------|
| **Angular** | SPA Framework | 21.0 |
| **TypeScript** | Type-safe JavaScript | 5.7 |
| **PrimeNG** | Enterprise UI Components | 18.0 |
| **PrimeFlex** | Utility-first CSS Framework | 3.x |
| **RxJS** | Reactive Programming | 7.8 |
| **@microsoft/signalr** | Real-time Client | 8.0 |

#### State Management Architecture
```typescript
// Signal-based Custom Store Pattern
export class AuthStore {
  private _user = signal<User | null>(null);
  private _loading = signal(false);
  
  user = this._user.asReadonly();
  isAuthenticated = computed(() => !!this._user());
  isBanker = computed(() => this._user()?.role === 'Banker');
}
```

#### UI/UX Features
- **Responsive Design** - Mobile-first with PrimeFlex grid system
- **Theme System** - PrimeNG Aura theme with CSS variables
- **Internationalization** - Multi-language support (English/Greek)
- **Toast Notifications** - Real-time user feedback
- **Confirmation Dialogs** - Critical action validation
- **Loading States** - Skeleton screens and spinners

---

## 🚀 Key Features & Implementation

### 1. Real-time Communication (SignalR)

**Backend Hub:**
```csharp
public class NotificationHub : Hub
{
    // User-specific notifications
    public async Task SendLoanNotification(string userId, string loanId, 
                                           string status, string message)
    
    // Broadcast to all bankers
    public async Task NotifyBankerNewSubmission(LoanApplication loan)
}
```

**Frontend Service:**
```typescript
export class SignalRService {
  private connection: signalR.HubConnection;
  
  notifications = signal<LoanNotification[]>([]);
  isConnected = signal(false);
  
  connect(): Promise<void> {
    return this.connection
      .start()
      .then(() => this.isConnected.set(true));
  }
}
```

**Use Cases:**
- ✅ Loan submission → Instant notification to bankers
- ✅ Approval/Rejection → Real-time update to applicant
- ✅ Status changes → Toast notifications across the app

### 2. Document Upload System

**File Validation:**
- Max size: 5MB
- Allowed types: PDF, DOC, DOCX, XLS, XLSX, JPG, JPEG, PNG
- Storage: `uploads/loans/{loanId}/filename`

**Implementation:**
```csharp
public class FileService
{
    public async Task<LoanDocument> SaveLoanDocumentAsync(int loanId, IFormFile file)
    {
        // Validate type and size
        if (!IsValidDocumentType(file.FileName))
            throw new ValidationException("Invalid file type");
            
        // Save with unique filename
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine("uploads", "loans", loanId.ToString(), fileName);
        
        // Store in database
        return new LoanDocument { FilePath = path, FileName = file.FileName };
    }
}
```

### 3. Email Notification Service

**Dual-mode Email System:**
- **Development:** Mock email (console logging)
- **Production:** SMTP integration (Gmail/SendGrid)

**Email Templates:**
```csharp
✓ Loan Submission Confirmation
✓ Approval Notification with Terms
✓ Rejection Notice with Feedback
```

**Configuration:**
```json
{
  "Email": {
    "UseMockEmail": true,
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "FromEmail": "noreply@creditflow.com"
  }
}
```

### 4. Audit Logging

**Every critical action is logged:**
```csharp
public enum AuditAction
{
    UserRegistered,
    UserLoggedIn,
    CreateLoanApplication,
    SubmitLoanApplication,
    ViewLoans,
    UploadDocument,
    ApproveLoan,
    RejectLoan
}
```

**Logged Information:**
- User ID
- Action type
- Timestamp (UTC)
- Additional context/metadata

---

## 📁 Solution Structure

```text
CreditFlow/
├── Base/                           # Infrastructure Layer
│   ├── Hubs/
│   │   └── NotificationHub.cs      # SignalR hub for real-time messaging
│   ├── Identity/
│   │   └── ApplicationUser.cs      # Custom user entity
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs  # Global error handling
│   ├── Persistance/
│   │   ├── AppDbContext.cs         # EF Core context
│   │   └── Seeding.cs              # Database seeding
│   └── Service/
│       ├── AuditService.cs         # Audit logging
│       ├── CurrentUserService.cs   # User context resolution
│       ├── FileService.cs          # Document management
│       ├── NotificationService.cs  # Email + SignalR
│       ├── TokenService.cs         # JWT generation
│       ├── LoanCalculationService.cs
│       └── RiskAssessmentService.cs
│
├── Domain/                         # Domain Layer
│   ├── Entities/
│   │   ├── LoanApplication.cs      # Rich domain model
│   │   ├── LoanDocument.cs
│   │   ├── AuditLog.cs
│   │   └── DTOs.cs                 # Data transfer objects
│   ├── Enums/
│   │   └── Enums.cs                # LoanStatus, AuditAction, etc.
│   └── Interfaces/
│       ├── IApplicationDbContext.cs
│       └── ICurrentUserService.cs
│
├── Feature/                        # Application Layer (CQRS)
│   └── Loans/
│       ├── Commands/
│       │   ├── CreateLoanCommand.cs
│       │   ├── SubmitLoanCommand.cs
│       │   ├── DecideLoan.cs
│       │   └── UploadDocumentCommand.cs
│       └── Queries/
│           ├── GetMyLoans.cs
│           ├── GetLoanByIdQuery.cs
│           └── GetPendingLoans.cs
│
├── Controllers/                    # Presentation Layer
│   ├── AuthController.cs           # Login/Register endpoints
│   ├── LoanController.cs           # Loan CRUD operations
│   └── BaseApiController.cs        # Common controller logic
│
├── Migrations/                     # EF Core migrations
├── Tests/                          # Unit & Integration tests
│   └── SignalRTests.cs
│
├── Angular/credit-flow-web/        # Frontend Application
│   └── src/app/
│       ├── core/                   # Singleton services & guards
│       │   ├── auth/
│       │   │   ├── auth-guard.ts
│       │   │   └── auth.interceptor.ts
│       │   ├── components/
│       │   │   └── notification-center/
│       │   ├── services/
│       │   │   ├── auth.service.ts
│       │   │   ├── loan.service.ts
│       │   │   └── signalr.service.ts
│       │   ├── stores/
│       │   │   ├── auth.store.ts
│       │   │   └── loan.store.ts
│       │   └── models/
│       │       └── models.ts
│       ├── features/               # Feature modules
│       │   ├── auth/
│       │   │   ├── login/
│       │   │   └── register/
│       │   ├── dashboard/          # Customer dashboard
│       │   └── banker/
│       │       └── banker-dashboard/
│       └── layout/                 # Structural components
│           ├── main-layout/
│           ├── header/
│           └── footer/
│
├── wwwroot/                        # Static files (build output - gitignored)
├── Program.cs                      # Startup & DI configuration
├── appsettings.json
└── CreditFlowAPI.csproj

```

---

## ⚙️ Configuration & Setup

### Prerequisites
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **SQL Server 2022** or SQL Server Express
- **Visual Studio 2022** or VS Code

### Backend Setup

1. **Clone the repository:**
```bash
git clone <repository-url>
cd CreditFlow
```

2. **Update connection string** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CreditFlowDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

3. **Apply migrations:**
```bash
dotnet ef database update
```

4. **Run the API:**
```bash
dotnet run
```
API runs on: `https://localhost:7001`

### Frontend Setup

1. **Navigate to Angular project:**
```bash
cd Angular/credit-flow-web
```

2. **Install dependencies:**
```bash
npm install
```

3. **Configure API endpoint** in `src/environments/`:
```typescript
export const environment = {
  apiUrl: 'https://localhost:7001/api'
};
```

4. **Run development server:**
```bash
npm start
```
App runs on: `http://localhost:4200`

---

## 🧪 Testing

### Backend Tests
```bash
dotnet test
```

**Test Coverage:**
- SignalR Hub functionality
- Notification service integration
- Email mock service
- CQRS command/query handlers

---

## 🔐 Security Best Practices

### Implemented Security Measures
- ✅ **JWT with short expiration** (60 minutes)
- ✅ **Refresh token support** (optional)
- ✅ **Password strength validation**
- ✅ **SQL injection prevention** (parameterized queries via EF Core)
- ✅ **XSS protection** (Angular's built-in sanitization)
- ✅ **CORS policy** (configurable origin whitelist)
- ✅ **HTTPS enforcement** in production
- ✅ **Concurrency handling** (Row versioning with `byte[]` RowVersion)

### Recommended Production Hardening
- [ ] Enable rate limiting
- [ ] Add CAPTCHA to registration
- [ ] Implement refresh token rotation
- [ ] Enable audit log encryption
- [ ] Configure Azure AD/OAuth integration

---

## 📊 Database Schema

### Core Entities

**ApplicationUser** (ASP.NET Identity)
- Email, PasswordHash, Role, PhoneNumber

**LoanApplication**
- Financial details (amount, term, interest rate)
- Employment information
- Calculated risk metrics
- Status (Draft → Submitted → Approved/Rejected)
- Concurrency token (RowVersion)

**LoanDocument**
- File metadata (name, path, size)
- Upload timestamp
- Foreign key to LoanApplication

**AuditLog**
- UserId, Action, Timestamp, Details
- Foreign key to ApplicationUser

---

## 🌐 API Endpoints

### Authentication
```http
POST /api/auth/register     # User registration
POST /api/auth/login        # User login (returns JWT)
```

### Loan Management (Authenticated)
```http
POST /api/loans/createloan       # Create loan application
POST /api/loans/getloans         # Get user's loans
POST /api/loans/getloandetails   # Get specific loan
POST /api/loans/submitloan       # Submit for review
POST /api/loans/uploaddocument   # Upload document
```

### Banker Operations (Banker Role)
```http
POST /api/loans/getpendingloans  # Get all pending loans
POST /api/loans/decideloan       # Approve/Reject loan
```

---

## 🎨 UI Components

### PrimeNG Components Used
- `p-card` - Card containers
- `p-table` - Data tables with sorting/filtering
- `p-button` - Action buttons
- `p-input-text` - Text inputs
- `p-select` - Dropdown selects
- `p-toast` - Notification toasts
- `p-dialog` - Modal dialogs
- `p-confirm-dialog` - Confirmation dialogs
- `p-file-upload` - File upload widget
- `p-badge` - Notification badges
- `p-progress-bar` - Loading indicators

---

## 🌍 Internationalization

**Supported Languages:**
- English (en)
- Greek (el)

**Implementation:**
```typescript
export class LanguagePipe implements PipeTransform {
  transform(key: string): string {
    const translations = this.languageService.getCurrentTranslations();
    return translations[key] || key;
  }
}
```

---

## 📈 Performance Optimizations

### Backend
- **Async/await** throughout for non-blocking I/O
- **EF Core compiled queries** for frequent operations
- **Response caching** for static endpoints
- **Connection pooling** for database connections

### Frontend
- **Lazy loading** for feature modules
- **OnPush change detection** with signals
- **Virtual scrolling** for large lists
- **Build optimization** (AOT compilation, tree shaking)

---

## 📝 License

This project is proprietary software developed for educational/portfolio purposes.

---

## 👨‍💻 Author

Developed with ❤️ using modern enterprise patterns and best practices.

**Tech Stack:** .NET 10 | Angular 21 | TypeScript | SignalR | SQL Server | PrimeNG

---

## 📚 Additional Documentation

- [SignalR Implementation Details](SIGNALR_IMPLEMENTATION.md)
- [Feature Implementation Summary](IMPLEMENTATION_SUMMARY.md)
- [Error Fixes & Troubleshooting](ERROR_FIXES_SUMMARY.md)
