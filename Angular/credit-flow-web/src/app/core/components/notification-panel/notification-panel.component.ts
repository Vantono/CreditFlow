import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { SignalRService, LoanNotification } from '../../services/signalr.service';

interface NotificationItem extends LoanNotification {
  id: string;
  isRead: boolean;
}

@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [CommonModule],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ]),
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'scale(0.95)' }))
      ])
    ])
  ],
  templateUrl: './notification-panel.component.html',
  styleUrl: './notification-panel.component.scss'
})
export class NotificationPanelComponent implements OnInit, OnDestroy {
  private signalRService = inject(SignalRService);

  isPanelOpen = signal(false);
  notificationItems = signal<NotificationItem[]>([]);
  unreadCount = signal(0);

  ngOnInit(): void {
    // Connect to SignalR
    this.signalRService.connect().catch(err =>
      console.error('Failed to connect to SignalR:', err)
    );

    // Watch for new notifications
    const notificationsEffect = setInterval(() => {
      const lastNotif = this.signalRService.getLastNotification()();
      if (lastNotif) {
        const item: NotificationItem = {
          ...lastNotif,
          id: `${lastNotif.timestamp.getTime()}-${Math.random()}`,
          isRead: false
        };

        const current = this.notificationItems();
        const updated = [item, ...current].slice(0, 50);
        this.notificationItems.set(updated);
        this.updateUnreadCount();
      }
    }, 100);

    (this as any)._notificationInterval = notificationsEffect;
  }

  ngOnDestroy(): void {
    this.signalRService.disconnect().catch(err =>
      console.error('Failed to disconnect from SignalR:', err)
    );
    
    if ((this as any)._notificationInterval) {
      clearInterval((this as any)._notificationInterval);
    }
  }

  togglePanel(): void {
    this.isPanelOpen.update(open => !open);
    if (this.isPanelOpen()) {
      // Mark all as read when opening panel
      this.notificationItems.update(items =>
        items.map(item => ({ ...item, isRead: true }))
      );
      this.updateUnreadCount();
    }
  }

  markAsRead(id: string): void {
    this.notificationItems.update(items =>
      items.map(item =>
        item.id === id ? { ...item, isRead: true } : item
      )
    );
    this.updateUnreadCount();
  }

  clearAll(): void {
    this.notificationItems.set([]);
    this.updateUnreadCount();
    this.signalRService.clearNotifications();
  }

  private updateUnreadCount(): void {
    const count = this.notificationItems().filter(item => !item.isRead).length;
    this.unreadCount.set(count);
  }

  getIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'success': '✅',
      'error': '❌',
      'warn': '⚠️',
      'info': 'ℹ️',
      'loan_status': '📋',
      'new_loan_submission': '📤'
    };
    return iconMap[type] || '📌';
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
