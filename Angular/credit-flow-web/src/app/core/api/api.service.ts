import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { 
  AuthResponse, 
  CreateLoanRequest, 
  DecideLoanRequest, 
  GetLoanDetailsRequest, 
  LoanDto, 
  LoginRequest, 
  PendingLoanDto, 
  RegisterRequest, 
  SubmitLoanRequest 
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  // Χρήση του inject() αντί για constructor (Modern Angular style)
  private http = inject(HttpClient);
  
  // API URLs - relative paths since Angular is served from the same server
  private apiAuthUrl = '/api/auth';
  private apiLoansUrl = '/api/loans';

  // ==========================================================
  // AUTHENTICATION
  // ==========================================================

  register(request: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiAuthUrl}/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiAuthUrl}/login`, request);
  }

  // ==========================================================
  // CUSTOMER LOANS (RPC Style - All POST)
  // ==========================================================

  getMyLoans(): Observable<LoanDto[]> {
    // Στέλνουμε κενό body {} επειδή ο Controller περιμένει JSON
    return this.http.post<LoanDto[]>(`${this.apiLoansUrl}/getloans`, {});
  }

  getLoanDetails(loanId: string): Observable<LoanDto> {
    const request: GetLoanDetailsRequest = { loanId };
    return this.http.post<LoanDto>(`${this.apiLoansUrl}/getloandetails`, request);
  }

  createLoan(request: CreateLoanRequest): Observable<string> { // Επιστρέφει το Guid ως string
    return this.http.post<string>(`${this.apiLoansUrl}/createloan`, request);
  }

  submitLoan(loanId: string): Observable<void> {
    const request: SubmitLoanRequest = { loanId };
    return this.http.post<void>(`${this.apiLoansUrl}/submitloan`, request);
  }

  // ==========================================================
  // BANKER AREA
  // ==========================================================

  getPendingLoans(): Observable<PendingLoanDto[]> {
    return this.http.post<PendingLoanDto[]>(`${this.apiLoansUrl}/getpendingloans`, {});
  }

  decideLoan(request: DecideLoanRequest): Observable<void> {
    return this.http.post<void>(`${this.apiLoansUrl}/decideloan`, request);
  }
}