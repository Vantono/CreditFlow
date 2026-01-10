import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';

// PrimeNG Modules
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';

import { ApiService } from '../../../core/api/api.service';
import { AuthStore } from '../../../core/auth/auth.store';
import { LoginRequest } from '../../../core/models/models';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    // PrimeNG
    CardModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    ToastModule,
    Message
],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  private authStore = inject(AuthStore);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // Σήμα για το αν φορτώνει (για να δείξουμε spinner στο κουμπί)
  isLoading = signal(false);
  
  errorMessage = signal<string | null>(null);

  // Η φόρμα μας
  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const request: LoginRequest = {
      email: this.loginForm.value.email!,
      password: this.loginForm.value.password!
    };

    this.api.login(request).subscribe({
      next: (response) => {
        // 1. Ενημερώνουμε το Store (και το localStorage)
        this.authStore.login(response);

        // 2. Ελέγχουμε αν υπήρχε returnUrl (από τον Guard)
        const returnUrl = this.route.snapshot.queryParams['returnUrl'];
        
        // 3. Πλοήγηση: Αν υπάρχει returnUrl πάμε εκεί, αλλιώς αποφασίζει το Store (Dashboard/Banker)
        if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        } 
        // Σημείωση: Το authStore.login() έχει ήδη logic για redirect, 
        // οπότε αν δεν υπάρχει returnUrl, θα το αναλάβει εκείνο.
      },
      error: () => {
        this.isLoading.set(false);
        // Εμφάνιση μηνύματος λάθους
        this.errorMessage.set('Invalid email or password.');
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }
}