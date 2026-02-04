// --- AUTH MODELS ---

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  taxId: string;
  dateOfBirth: string; // ISO date string
  phoneNumber: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  userName: string;
}

// --- LOAN MODELS ---

export enum LoanStatus {
  Draft = 1,
  Submitted = 2,
  UnderReview = 3,
  Approved = 4,
  Rejected = 5
}

export enum LoanStatusText {
  Draft = 'Draft',
  Submitted = 'Submitted',
  UnderReview = 'UnderReview',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Completed = 'Completed'
}

export enum RiskLevel {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}

export interface CreateLoanRequest {
  loanAmount: number;
  termMonths: number;
  purpose: string;
  employerName: string;
  jobTitle: string;
  yearsEmployed: number;
  monthlyIncome: number;
  monthlyExpenses: number;
}

export interface LoanDto {
  id: string; // GUID is string in JS
  loanAmount: number;
  termMonths: number;
  purpose: string;
  status: LoanStatusText;
  statusCode: LoanStatus;
  createdAt: string;
  applicantName?: string;
  interestRate?: number;
  monthlyPayment?: number;
  totalInterest?: number;
  debtToIncomeRatio?: number;
  riskLevel?: RiskLevel;
}

export interface SubmitLoanRequest {
  loanId: string;
}

export interface GetLoanDetailsRequest {
  loanId: string;
}

// --- BANKER MODELS ---

export interface PendingLoanDto {
  id: string;
  applicantId: string;
  applicantName: string;
  applicantEmail: string;
  amount: number;
  termMonths: number;
  purpose: string;
  employerName: string;
  jobTitle: string;
  yearsEmployed: number;
  monthlyIncome: number;
  monthlyExpenses: number;
  interestRate: number;
  monthlyPayment: number;
  totalInterest: number;
  debtToIncomeRatio: number;
  riskLevel: RiskLevel;
  submittedOn: string;
  rowVersion: string; // base64 encoded
}

export enum DecisionType {
  Approve = 0,
  Reject = 1
}

export enum Severity{
  Success = 'success',
  Info = 'info',
  Warn = 'warn',
  Danger = 'danger'
}

export enum RiskSeverity{
  Success = 'success',
  Warn = 'warn',
  Danger = 'danger'
}

export enum Language {
  English = 'EN',
  Greek = 'GR'
}

export interface DecideLoanRequest {
  loanId: string;
  approved: boolean;
  comments?: string;
  decision?: DecisionType;
  rowVersion?: string; // base64 encoded concurrency token
}