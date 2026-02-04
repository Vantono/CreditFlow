import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from '../header/header';
import { Footer } from '../footer/footer';
import { NotificationCenterComponent } from '../../core/components/notification-center/notification-center.component';
import { SignalRService } from '../../core/services/signalr.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, Header, Footer, NotificationCenterComponent],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout implements OnInit, OnDestroy {
  private signalR = inject(SignalRService);

  ngOnInit() {
    this.signalR.connect().catch(err =>
      console.error('SignalR connection failed:', err)
    );
  }

  ngOnDestroy() {
    this.signalR.disconnect();
  }
}
