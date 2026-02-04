import { Component, inject, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

// PrimeNG Modules
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';
import { Tooltip } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';

import { LoanStore } from './loan.store';
import { SignalRService } from '../../core/services/signalr.service';
import { CreateLoanRequest, LoanDto, LoanStatus, RiskLevel, RiskSeverity, Severity } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputNumber,
    Tag,
    ConfirmDialog,
    Toast,
    Tooltip
],
  providers: [ConfirmationService, MessageService],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private fb = inject(FormBuilder);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Stores & Services
  protected loanStore = inject(LoanStore);
  private signalR = inject(SignalRService);

  // Auto-refresh loans when banker approves/rejects via SignalR
  protected loanStatusEffect = effect(() => {
    const notification = this.signalR.lastNotification();
    if (notification?.type === 'loan_status') {
      this.loanStore.loadLoans();
    }
  });

  // UI State
  showCreateDialog = signal(false);
  showDetailsDialog = signal(false);
  isSubmitting = signal(false);

  // Form for creating new loan
  loanForm = this.fb.group({
    // Loan Details
    loanAmount: [null as number | null, [Validators.required, Validators.min(100), Validators.max(1000000)]],
    termMonths: [null as number | null, [Validators.required, Validators.min(1), Validators.max(360)]],
    purpose: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],

    // Employment Information
    employerName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    jobTitle: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    yearsEmployed: [null as number | null, [Validators.required, Validators.min(0), Validators.max(60)]],

    // Financial Information
    monthlyIncome: [null, [Validators.required, Validators.min(0), Validators.max(1000000)]],
    monthlyExpenses: [ null, [Validators.required, Validators.min(0), Validators.max(1000000)]]
  });

  // Expose LoanStatus enum to template
  readonly LoanStatus = LoanStatus;

  ngOnInit() {
    // Load loans when component initializes
    this.loanStore.loadLoans();
  }

  // ==========================================================
  // Actions
  // ==========================================================

  openCreateDialog() {
    this.loanForm.reset();
    this.showCreateDialog.set(true);
  }

  closeCreateDialog() {
    this.showCreateDialog.set(false);
    this.loanForm.reset();
  }

  createLoan() {
    if (this.loanForm.invalid) {
      return;
    }

    this.isSubmitting.set(true);

    const request: CreateLoanRequest = {
      loanAmount: this.loanForm.value.loanAmount!,
      termMonths: this.loanForm.value.termMonths!,
      purpose: this.loanForm.value.purpose!,
      employerName: this.loanForm.value.employerName!,
      jobTitle: this.loanForm.value.jobTitle!,
      yearsEmployed: this.loanForm.value.yearsEmployed!,
      monthlyIncome: this.loanForm.value.monthlyIncome!,
      monthlyExpenses: this.loanForm.value.monthlyExpenses!
    };

    this.loanStore.createLoan(request).subscribe({
      next: () => {
        this.messageService.add({
          severity: Severity.Success,
          summary: 'Success',
          detail: 'Loan application created successfully'
        });
        this.closeCreateDialog();
      },
      error: (err) => {
        this.messageService.add({
          severity: Severity.Danger,
          summary: 'Error',
          detail: err.enhancedError?.message || 'Failed to create loan'
        });
      },
      complete: () => {
        this.isSubmitting.set(false);
      }
    });
  }

  viewLoanDetails(loan: LoanDto) {
    this.loanStore.selectLoan(loan);
    this.showDetailsDialog.set(true);
  }

  closeDetailsDialog() {
    this.showDetailsDialog.set(false);
    this.loanStore.selectLoan(null);
  }

  submitLoan(loan: LoanDto) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to submit this loan application for review?',
      header: 'Confirm Submission',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loanStore.submitLoan(loan.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Loan submitted for review'
            });
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.enhancedError?.message || 'Failed to submit loan'
            });
          }
        });
      }
    });
  }

  // ==========================================================
  // Helpers
  // ==========================================================

  getSeverity(status: LoanStatus): Severity {
    switch (status) {
      case LoanStatus.Draft:
        return Severity.Warn;
      case LoanStatus.Submitted:
      case LoanStatus.UnderReview:
        return Severity.Info;
      case LoanStatus.Approved:
        return Severity.Success;
      case LoanStatus.Rejected:
        return Severity.Danger;
      default:
        return Severity.Info;
    }
  }

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

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

 formatDate(dateTime: string): string {
  if (!dateTime) {
    return '';
  }

  const date = new Date(dateTime);

  if (isNaN(date.getTime())) {
    return '';
  }

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();

  return `${day}/${month}/${year}`;
}

}
