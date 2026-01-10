// --- AUTH MODELS ---

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
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
  Pending = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3
}

export interface CreateLoanRequest {
  loanAmount: number;
  termMonths: number;
  purpose: string;
}

export interface LoanDto {
  id: string; // GUID is string in JS
  loanAmount: number;
  termMonths: number;
  purpose: string;
  statusName: string;
  statusCode: LoanStatus;
  createdOnUtc: string; 
  applicantName?: string;
}

export interface SubmitLoanRequest {
  loanId: string;
}

export interface GetLoanDetailsRequest {
  loanId: string;
}

// --- BANKER MODELS ---

export interface PendingLoanDto {
  loanId: string;
  amount: number;
  term: number;
  purpose: string;
  date: string;
  applicantName: string;
  email: string;
}

export enum DecisionType {
  Approve = 0,
  Reject = 1
}

export interface DecideLoanRequest {
  loanId: string;
  approved: boolean;
  comments?: string;
  decision?: DecisionType; 
}