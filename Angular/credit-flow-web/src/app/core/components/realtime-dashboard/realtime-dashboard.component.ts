import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { LoanStatusText } from '../../models/models';
import { SignalRService, LoanNotification } from '../../services/signalr.service';

interface LoanStatus {
  id: string;
  title: string;
  amount: number;
  status: LoanStatusText;
  progress: number;
  lastUpdate: Date;
  applicantName: string;
  isLoading?: boolean;
}

@Component({
  selector: 'app-realtime-dashboard',
  standalone: true,
  imports: [CommonModule],
  animations: [
    trigger('slideUp', [
      transition(':enter', [
        style({ transform: 'translateY(20px)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateY(0)', opacity: 1 }))
      ])
    ]),
    trigger('pulse', [
      transition('* => *', [
        animate('600ms ease-in-out', style({ transform: 'scale(1)' })),
      ])
    ])
  ],
  templateUrl: './realtime-dashboard.component.html',
  styleUrl: './realtime-dashboard.component.scss'
})
export class RealtimeDashboardComponent implements OnInit {
  signalRService = inject(SignalRService);
  readonly loanStatusText = LoanStatusText;

  loanApplications = signal<LoanStatus[]>([
    {
      id: '1',
      title: 'Home Loan',
      amount: 250000,
      status: LoanStatusText.Submitted,
      progress: 60,
      lastUpdate: new Date(Date.now() - 2 * 60000),
      applicantName: 'John Doe'
    },
    {
      id: '2',
      title: 'Personal Loan',
      amount: 50000,
      status: LoanStatusText.Draft,
      progress: 30,
      lastUpdate: new Date(Date.now() - 30 * 60000),
      applicantName: 'Jane Smith'
    },
    {
      id: '3',
      title: 'Business Loan',
      amount: 500000,
      status: LoanStatusText.Approved,
      progress: 100,
      lastUpdate: new Date(Date.now() - 5 * 60000),
      applicantName: 'Acme Corp'
    }
  ]);

  recentActivity = computed(() => {
    const notifications = this.signalRService.getNotifications()();
    return notifications.slice(0, 10).map(notif => ({
      type: notif.type,
      title: notif.title,
      message: notif.message,
      timestamp: notif.timestamp
    }));
  });

  approvedCount = computed(() => 
    this.loanApplications().filter(l => l.status === LoanStatusText.Approved).length
  );

  pendingCount = computed(() => 
    this.loanApplications().filter(l => l.status === LoanStatusText.Submitted).length
  );

  totalApproved = computed(() =>
    this.loanApplications()
      .filter(l => l.status === LoanStatusText.Approved)
      .reduce((sum, l) => sum + l.amount, 0)
  );

  ngOnInit(): void {
    this.signalRService.connect().catch(err =>
      console.error('Failed to connect to SignalR:', err)
    );
  }

  viewDetails(loan: LoanStatus): void {
    console.log('Viewing details for loan:', loan.id);
  }

  submitApplication(loan: LoanStatus): void {
    loan.isLoading = true;
    setTimeout(() => {
      loan.status = LoanStatusText.Submitted;
      loan.progress = 60;
      loan.lastUpdate = new Date();
      loan.isLoading = false;
    }, 1500);
  }

  approveApplication(loan: LoanStatus): void {
    loan.isLoading = true;
    setTimeout(() => {
      loan.status = LoanStatusText.Approved;
      loan.progress = 100;
      loan.lastUpdate = new Date();
      loan.isLoading = false;
    }, 1500);
  }

  rejectApplication(loan: LoanStatus): void {
    loan.isLoading = true;
    setTimeout(() => {
      loan.status = LoanStatusText.Rejected;
      loan.progress = 100;
      loan.lastUpdate = new Date();
      loan.isLoading = false;
    }, 1500);
  }

  getActivityIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'success': '✅',
      'error': '❌',
      'warn': '⚠️',
      'info': 'ℹ️',
      'loan_status': '📋',
      'new_loan_submission': '📤'
    };
    return icons[type] || '📌';
  }

  formatTime(timestamp: Date): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diff = now.getTime() - date.getTime();

    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'just now';
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;

    return date.toLocaleDateString();
  }
}
