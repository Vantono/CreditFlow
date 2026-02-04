import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { SignalRService } from '../../services/signalr.service';

interface Toast {
  id: string;
  title: string;
  message: string;
  type: 'success' | 'error' | 'warn' | 'info';
  duration?: number;
}

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  imports: [CommonModule],
  animations: [
    trigger('slideInOut', [
      transition(':enter', [
        style({ transform: 'translateX(400px)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ transform: 'translateX(400px)', opacity: 0 }))
      ])
    ]),
    trigger('autoClose', [
      transition(':leave', [
        animate('100ms ease-out', style({ opacity: 0.8 }))
      ])
    ])
  ],
  templateUrl: './notification-toast.component.html',
  styleUrl: './notification-toast.component.scss'
})
export class NotificationToastComponent implements OnInit, OnDestroy {
  private signalRService = inject(SignalRService);

  toasts = signal<Toast[]>([]);
  private toastTimers = new Map<string, any>();

  ngOnInit(): void {
    // Subscribe to notifications
    const checkInterval = setInterval(() => {
      const lastNotif = this.signalRService.getLastNotification()();
      if (lastNotif) {
        this.showToast({
          title: lastNotif.title,
          message: lastNotif.message,
          type: this.mapNotificationType(lastNotif.type),
          duration: 5000
        });
      }
    }, 100);

    (this as any)._checkInterval = checkInterval;
  }

  ngOnDestroy(): void {
    if ((this as any)._checkInterval) {
      clearInterval((this as any)._checkInterval);
    }
    
    // Clear all timers
    this.toastTimers.forEach(timer => clearTimeout(timer));
    this.toastTimers.clear();
  }

  showToast(toast: Omit<Toast, 'id'>): void {
    const id = `${Date.now()}-${Math.random()}`;
    const newToast: Toast = { ...toast, id };

    this.toasts.update(toasts => [...toasts, newToast]);

    if (toast.duration) {
      const timer = setTimeout(() => {
        this.removeToast(id);
      }, toast.duration);

      this.toastTimers.set(id, timer);
    }
  }

  removeToast(id: string): void {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));

    const timer = this.toastTimers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.toastTimers.delete(id);
    }
  }

  private mapNotificationType(type: string): 'success' | 'error' | 'warn' | 'info' {
    const typeMap: { [key: string]: 'success' | 'error' | 'warn' | 'info' } = {
      'success': 'success',
      'error': 'error',
      'warn': 'warn',
      'info': 'info',
      'loan_status': 'info',
      'new_loan_submission': 'info'
    };
    return typeMap[type] || 'info';
  }

  getIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'success': '✅',
      'error': '❌',
      'warn': '⚠️',
      'info': 'ℹ️'
    };
    return icons[type] || '📌';
  }
}
