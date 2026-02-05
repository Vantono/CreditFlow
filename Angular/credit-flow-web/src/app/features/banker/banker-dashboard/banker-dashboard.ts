import { Component, OnInit, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

// PrimeNG Imports
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { Tooltip } from 'primeng/tooltip';
import { Dialog } from 'primeng/dialog';
import { Textarea } from 'primeng/textarea';
import { Card } from 'primeng/card';
import { BankerStore } from '../banker.store';
import { SignalRService } from '../../../core/services/signalr.service';
import { DecideLoanRequest, LoanStatusText, PendingLoanDto, RiskLevel, RiskSeverity } from '../../../core/models/models';
import { LanguagePipe } from '../../../core/Language/language.pipe';

@Component({
  selector: 'app-banker-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    Tag,
    ConfirmDialogModule,
    ToastModule,
    Tooltip,
    Dialog,
    Textarea,
    Card,
    LanguagePipe
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './banker-dashboard.html',
  styleUrl: './banker-dashboard.scss'
})
export class BankerDashboardComponent implements OnInit {
  private fb = inject(FormBuilder);
  store = inject(BankerStore);
  private signalR = inject(SignalRService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Auto-refresh pending loans when a new submission arrives via SignalR
  protected newSubmissionEffect = effect(() => {
    const notification = this.signalR.lastNotification();
    if (notification?.type === 'new_loan_submission') {
      this.store.loadPendingLoans();
    }
  });

  // UI State
  showDetailsDialog = signal(false);
  selectedLoan = signal<PendingLoanDto | null>(null);
  showDecisionDialog = signal(false);
  decisionType = signal<'approve' | 'reject' | null>(null);

  // Decision form
  decisionForm = this.fb.group({
    comments: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]]
  });

  ngOnInit() {
    this.store.loadPendingLoans();
  }

  // View loan details
  viewDetails(loan: PendingLoanDto) {
    this.selectedLoan.set(loan);
    this.showDetailsDialog.set(true);
  }

  closeDetailsDialog() {
    this.showDetailsDialog.set(false);
  }

  // Open decision dialog
  openApproveDialog(loan: PendingLoanDto) {
    this.selectedLoan.set(loan);
    this.decisionType.set('approve');
    this.decisionForm.reset();
    this.showDecisionDialog.set(true);
  }

  openRejectDialog(loan: PendingLoanDto) {
    this.selectedLoan.set(loan);
    this.decisionType.set('reject');
    this.decisionForm.reset();
    this.showDecisionDialog.set(true);
  }

  closeDecisionDialog() {
    this.showDecisionDialog.set(false);
    this.decisionType.set(null);
    this.selectedLoan.set(null);
    this.decisionForm.reset();
  }

  // Submit decision
  submitDecision() {
    if (this.decisionForm.invalid || !this.selectedLoan() || !this.decisionType()) {
      return;
    }

    const loan = this.selectedLoan()!;
    const isApproval = this.decisionType() === 'approve';

    const req: DecideLoanRequest = {
      loanId: loan.id,
      approved: isApproval,
      comments: this.decisionForm.value.comments!,
      rowVersion: loan.rowVersion
    };

    this.store.decide(req);

    this.messageService.add({
      severity: isApproval ? 'success' : 'info',
      summary: isApproval ? LoanStatusText.Approved : LoanStatusText.Rejected,
      detail: isApproval
        ? `Loan for ${loan.applicantName} approved successfully.`
        : `Loan for ${loan.applicantName} rejected.`
    });

    this.closeDecisionDialog();
    this.closeDetailsDialog();
  }

  // Risk severity mapping
  getRiskSeverity(riskLevel: RiskLevel): RiskSeverity {
    switch (riskLevel) {
      case RiskLevel.Low:
        return RiskSeverity.Success;
      case RiskLevel.Medium:
        return RiskSeverity.Warn;
      case RiskLevel.High:
        return RiskSeverity.Danger;
      default:
        return RiskSeverity.Warn;
    }
  }

  // Helpers
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatDate(dateString: string): string {
    const truncatedDate = dateString.replace(/(\.\d{3})\d+/, '$1');
    return new Date(truncatedDate).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatPercent(value: number): string {
    return `${value.toFixed(1)}%`;
  }

}
