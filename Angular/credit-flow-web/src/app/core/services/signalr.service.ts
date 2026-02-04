import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { LoanStatusText } from '../models/models';

export interface LoanNotification {
  type: string;
  loanId?: string;
  status?: LoanStatusText;
  title: string;
  message: string;
  timestamp: Date;
  additionalData?: any;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private http = inject(HttpClient);
  private hubConnection: signalR.HubConnection | null = null;
  
  notifications = signal<LoanNotification[]>([]);
  isConnected = signal(false);
  lastNotification = signal<LoanNotification | null>(null);

  private readonly HUB_URL = '/hubs/notifications';
  private connectionPromise: Promise<void> | null = null;

  constructor() {
    // Lazy initialization - don't initialize in constructor
  }

  private initializeConnection(): void {
    if (!this.hubConnection) {
      try {
        this.hubConnection = new signalR.HubConnectionBuilder()
          .withUrl(this.HUB_URL, {
            accessTokenFactory: () => this.getToken()
          })
          .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: (retryContext: signalR.RetryContext) => {
              if (retryContext.previousRetryCount === 0) return 0;
              if (retryContext.previousRetryCount === 1) return 2000;
              if (retryContext.previousRetryCount === 2) return 10000;
              return 30000;
            }
          })
          .configureLogging(signalR.LogLevel.Warning)
          .build();

        this.setupEventHandlers();
      } catch (err) {
        console.error('Failed to initialize SignalR connection:', err);
      }
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      const typedNotification: LoanNotification = {
        type: notification.type,
        title: notification.title,
        message: notification.message,
        timestamp: new Date(notification.timestamp),
        additionalData: notification.additionalData
      };
      this.handleNotification(typedNotification);
    });

    this.hubConnection.on('ReceiveLoanNotification', (notification: any) => {
      const typedNotification: LoanNotification = {
        type: notification.type,
        loanId: notification.loanId,
        status: notification.status,
        title: notification.title,
        message: notification.message,
        timestamp: new Date(notification.timestamp),
        additionalData: notification.additionalData
      };
      this.handleNotification(typedNotification);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.isConnected.set(true);
    });

    this.hubConnection.onreconnecting((err: Error | undefined) => {
      console.log('SignalR reconnecting...', err);
      this.isConnected.set(false);
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR disconnected');
      this.isConnected.set(false);
    });
  }

  async connect(): Promise<void> {
    if (!this.hubConnection) {
      this.initializeConnection();
    }

    if (!this.hubConnection || this.connectionPromise) {
      return this.connectionPromise || Promise.resolve();
    }

    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.isConnected.set(true);
      return;
    }

    try {
      this.connectionPromise = this.hubConnection.start();
      await this.connectionPromise;
      this.isConnected.set(true);
      console.log('SignalR connected');
    } catch (err) {
      console.error('SignalR connection failed:', err);
      this.isConnected.set(false);
      throw err;
    } finally {
      this.connectionPromise = null;
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        this.isConnected.set(false);
        console.log('SignalR disconnected');
      } catch (err) {
        console.error('SignalR disconnection failed:', err);
      }
    }
  }

  async sendTestMessage(message: string): Promise<void> {
    if (!this.hubConnection || !this.isConnected()) {
      throw new Error('SignalR not connected');
    }

    try {
      await this.hubConnection.invoke('SendTestMessage', message);
    } catch (err) {
      console.error('Failed to send test message:', err);
      throw err;
    }
  }

  async sendLoanNotification(userId: string, loanId: string, status: string, message: string): Promise<void> {
    if (!this.hubConnection || !this.isConnected()) {
      throw new Error('SignalR not connected');
    }

    try {
      await this.hubConnection.invoke('SendLoanNotification', userId, loanId, status, message);
    } catch (err) {
      console.error('Failed to send loan notification:', err);
      throw err;
    }
  }

  private handleNotification(notification: LoanNotification): void {
    this.lastNotification.set(notification);
    const currentNotifications = this.notifications();
    // Keep only last 50 notifications
    const updatedNotifications = [notification, ...currentNotifications].slice(0, 50);
    this.notifications.set(updatedNotifications);
    
    console.log('Notification received:', notification);
  }

  getNotifications() {
    return this.notifications;
  }

  getLastNotification() {
    return this.lastNotification;
  }

  private getToken(): string {
    const stored = localStorage.getItem('creditflow_user');
    if (!stored) return '';
    try {
      return JSON.parse(stored).token || '';
    } catch {
      return '';
    }
  }

  clearNotifications(): void {
    this.notifications.set([]);
  }
}
