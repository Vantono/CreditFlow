import { Injectable, computed, inject, signal } from '@angular/core';
import { ApiService } from '../../core/api/api.service';
import { CreateLoanRequest, LoanDto, LoanStatus, LoanStatusText } from '../../core/models/models';
import { tap, finalize } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class LoanStore {
  private api = inject(ApiService);

  // ==========================================================
  // STATE (Private Signals)
  // ==========================================================

  private _loans = signal<LoanDto[]>([]);
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);
  private _selectedLoan = signal<LoanDto | null>(null);

  // ==========================================================
  // SELECTORS (Public Read-Only Signals)
  // ==========================================================

  // All loans
  readonly loans = this._loans.asReadonly();

  // Loading state
  readonly isLoading = this._isLoading.asReadonly();

  // Error state
  readonly error = this._error.asReadonly();

  // Selected loan for details view
  readonly selectedLoan = this._selectedLoan.asReadonly();

  // Computed: Loans grouped by status
  readonly draftLoans = computed(() =>
    this._loans().filter(loan => loan.statusCode === LoanStatus.Draft)
  );

  readonly submittedLoans = computed(() =>
    this._loans().filter(loan => loan.statusCode === LoanStatus.Submitted)
  );

  readonly approvedLoans = computed(() =>
    this._loans().filter(loan => loan.statusCode === LoanStatus.Approved)
  );

  readonly rejectedLoans = computed(() =>
    this._loans().filter(loan => loan.statusCode === LoanStatus.Rejected)
  );

  // Computed: Statistics
  readonly stats = computed(() => ({
    total: this._loans().length,
    pending: this.draftLoans().length,
    submitted: this.submittedLoans().length,
    approved: this.approvedLoans().length,
    rejected: this.rejectedLoans().length,
    totalAmount: this._loans().reduce((sum, loan) => sum + loan.loanAmount, 0),
    approvedAmount: this.approvedLoans().reduce((sum, loan) => sum + loan.loanAmount, 0)
  }));

  // Computed: Has loans
  readonly hasLoans = computed(() => this._loans().length > 0);

  // ==========================================================
  // ACTIONS (Methods that modify state)
  // ==========================================================

  /**
   * Load all loans for the current user
   */
  loadLoans() {
    this._isLoading.set(true);
    this._error.set(null);

    this.api.getMyLoans().pipe(
      tap(loans => {
        this._loans.set(loans);
      }),
      finalize(() => this._isLoading.set(false))
    ).subscribe({
      error: (err) => {
        console.error('Failed to load loans:', err);
        this._error.set(err.enhancedError?.message || 'Failed to load loans');
      }
    });
  }

  /**
   * Create a new loan
   */
  createLoan(request: CreateLoanRequest) {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.createLoan(request).pipe(
      tap(loanId => {
        console.log('Loan created:', loanId);
        // Reload loans to get the new one
        this.loadLoans();
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Submit a loan for review
   */
  submitLoan(loanId: string) {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.submitLoan(loanId).pipe(
      tap(() => {
        console.log('Loan submitted:', loanId);
        // Update the loan status locally
        this._loans.update(loans =>
          loans.map(loan =>
            loan.id === loanId
              ? { ...loan, statusCode: LoanStatus.Submitted, status: LoanStatusText.Submitted }
              : loan
          )
        );
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Load details for a specific loan
   */
  loadLoanDetails(loanId: string) {
    this._isLoading.set(true);
    this._error.set(null);

    this.api.getLoanDetails(loanId).pipe(
      tap(loan => {
        this._selectedLoan.set(loan);
      }),
      finalize(() => this._isLoading.set(false))
    ).subscribe({
      error: (err) => {
        console.error('Failed to load loan details:', err);
        this._error.set(err.enhancedError?.message || 'Failed to load loan details');
      }
    });
  }

  /**
   * Select a loan for viewing details
   */
  selectLoan(loan: LoanDto | null) {
    this._selectedLoan.set(loan);
  }

  /**
   * Clear error state
   */
  clearError() {
    this._error.set(null);
  }

  /**
   * Reset store state
   */
  reset() {
    this._loans.set([]);
    this._selectedLoan.set(null);
    this._error.set(null);
    this._isLoading.set(false);
  }
}
