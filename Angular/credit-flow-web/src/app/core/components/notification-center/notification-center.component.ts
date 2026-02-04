import { Component, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { LoanStatusText } from '../../models/models';
import { SignalRService, LoanNotification } from '../../services/signalr.service';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule, ToastModule],
  providers: [MessageService],
  templateUrl: './notification-center.component.html'
})
export class NotificationCenterComponent {
  private signalRService = inject(SignalRService);
  private messageService = inject(MessageService);

  constructor() {
    effect(() => {
      const lastNotification = this.signalRService.lastNotification();
      if (lastNotification) {
        this.displayNotification(lastNotification);
      }
    });
  }

  private displayNotification(notification: LoanNotification): void {
    const severityMap: { [key: string]: 'success' | 'info' | 'warn' | 'error' } = {
      'success': 'success',
      'warn': 'warn',
      'error': 'error',
      'info': 'info',
      'loan_status': notification.status === LoanStatusText.Approved ? 'success' : 'warn',
      'new_loan_submission': 'info'
    };

    const severity = severityMap[notification.type] || 'info';

    this.messageService.add({
      severity: severity,
      summary: notification.title,
      detail: notification.message,
      life: 5000
    });
  }
}
