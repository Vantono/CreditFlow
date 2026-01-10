# CreditFlow | Enterprise Loan Management System

CreditFlow is a comprehensive, full-stack enterprise application engineered to streamline the end-to-end loan application and approval lifecycle. It facilitates secure and efficient interaction between applicants and financial institution personnel, adhering to strict security standards and scalable architectural patterns.

## Project Overview

The system addresses the complete loan lifecycle through specific functional modules:

1.  **Identity & Access Management:** Secure user registration and authentication utilizing Role-Based Access Control (RBAC).
2.  **Customer Portal:** A dedicated interface for applicants to submit loan requests via a multi-step wizard and monitor application status in real-time.
3.  **Banker Console:** An administrative dashboard for reviewing pending applications and executing approval or rejection decisions.
4.  **Audit Trail:** Comprehensive logging of all critical system actions to ensure compliance and traceability.

## Technical Architecture

### Backend (.NET 10)
The backend is constructed using **Clean Architecture** principles to enforce separation of concerns, maintainability, and testability.

* **Framework:** ASP.NET Core 10 Web API.
* **Architectural Pattern:** CQRS (Command Query Responsibility Segregation) implemented via **MediatR**.
* **Data Persistence:** Entity Framework Core (Code-First approach) with SQL Server.
* **Security:** ASP.NET Identity, JWT Authentication (Bearer Tokens), and granular Role-Based Authorization.
* **Validation:** Server-side validation using FluentValidation and centralized Exception Handling Middleware.
* **API Design:** RPC-style endpoints with strict Data Transfer Object (DTO) contracts.

### Frontend (Angular 21)
The frontend is a high-performance Single Page Application (SPA) leveraging modern reactive paradigms.

* **Framework:** Angular 21 utilizing Standalone Components.
* **State Management:** **Angular Signals** (Custom Store Pattern), eliminating the need for complex external libraries like NGRX.
* **Component Library:** **PrimeNG** (Aura Theme) for enterprise-grade UI elements.
* **Responsiveness:** PrimeFlex grid system ensuring mobile-first compatibility.
* **Form Management:** Strictly typed Reactive Forms for robust data entry.

## Functional Modules

### Authentication & Security
* Secure Login and Registration flows using JSON Web Tokens (JWT).
* Automatic token injection via HTTP Interceptors.
* **Route Guards:** Enforce role boundaries, ensuring strictly separated contexts for Customers and Bankers.

### Customer Features
* **Dashboard:** Real-time view of requested loans and their current status (Pending, Approved, Rejected).
* **Loan Application Wizard:** A guided, multi-step interface for data entry and submission.

### Banker Features
* **Review Interface:** Aggregated view of pending loan applications.
* **Decision Engine:** Functionality to approve or reject loans with mandatory commentary.
* **Data Integrity:** Read-only access to applicant profile data to prevent unauthorized modification.

## Solution Structure

The solution adheres to a modular directory structure:

```text
CreditFlow/
├── CreditFlowAPI/           # Backend Solution (.NET 10)
│   ├── Base/                # Shared Kernel (Identity, Infrastructure, Seeding)
│   ├── Domain/              # Business Entities & Enums
│   ├── Features/            # Vertical Slices (CQRS Handlers)
│   └── Program.cs           # Dependency Injection & Pipeline Configuration
│
└── CreditFlowWeb/           # Frontend Solution (Angular 21)
    ├── src/app/core/        # Singleton Services, Signal Stores, Interceptors
    ├── src/app/features/    # Feature Modules (Auth, Dashboard, Banker)
    └── src/app/layout/      # Structural Components (Header, Footer)