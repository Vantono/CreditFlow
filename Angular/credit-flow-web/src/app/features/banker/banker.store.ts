import { Injectable, computed, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { ApiService } from '../../core/api/api.service';
import { DecideLoanRequest, PendingLoanDto, RiskLevel } from '../../core/models/models';

@Injectable({
  providedIn: 'root'
})
export class BankerStore {
  private api = inject(ApiService);

  // --- STATE ---
  private _pendingLoans = signal<PendingLoanDto[]>([]);
  private _isLoading = signal<boolean>(false);

  // --- SELECTORS ---
  readonly pendingLoans = this._pendingLoans.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  
  // Computed: Πόσα λεφτά ζητάνε συνολικά σε εκκρεμότητα (CRM Metric)
  readonly totalPendingAmount = computed(() =>
    this._pendingLoans().reduce((sum, loan) => sum + loan.amount, 0)
  );

  // Computed: Count by risk level
  readonly lowRiskCount = computed(() =>
    this._pendingLoans().filter(loan => loan.riskLevel === RiskLevel.Low).length
  );

  readonly mediumRiskCount = computed(() =>
    this._pendingLoans().filter(loan => loan.riskLevel === RiskLevel.Medium).length
  );

  readonly highRiskCount = computed(() =>
    this._pendingLoans().filter(loan => loan.riskLevel === RiskLevel.High).length
  );

  // --- ACTIONS ---
  
  loadPendingLoans() {
    this._isLoading.set(true);
    this.api.getPendingLoans()
      .pipe(finalize(() => this._isLoading.set(false)))
      .subscribe({
        next: (loans) => this._pendingLoans.set(loans),
        error: (err) => console.error('Failed to load loans', err)
      });
  }

  decide(request: DecideLoanRequest) {
    this._isLoading.set(true);
    this.api.decideLoan(request)
      .pipe(finalize(() => this._isLoading.set(false)))
      .subscribe({
        next: () => {
          // Optimistic Update: Αφαιρούμε το δάνειο από τη λίστα χωρίς να ξαναρωτήσουμε το API
          this._pendingLoans.update(loans =>
            loans.filter(l => l.id !== request.loanId)
          );
        },
        error: (err) => alert('Error submitting decision')
      });
  }
}