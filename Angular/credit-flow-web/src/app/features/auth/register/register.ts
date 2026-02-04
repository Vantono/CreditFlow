import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

// PrimeNG Modules
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { Message } from 'primeng/message';
import { InputMask } from 'primeng/inputmask';
import { DatePicker } from 'primeng/datepicker';

import { ApiService } from '../../../core/api/api.service';
import { RegisterRequest } from '../../../core/models/models';

@Component({
  selector: 'app-register',
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
    Message,
    InputMask,
    DatePicker
],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  private router = inject(Router);

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  currentStep = signal(0);
  maxDate = new Date(); // For date picker validation

  // Registration form with validation
  registerForm = this.fb.group({
    // Step 1: Basic Info
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],

    // Step 2: KYC Info
    taxId: ['', [Validators.required, Validators.pattern(/^\d{3}-\d{2}-\d{4}$/)]],
    dateOfBirth: [null as Date | null, [Validators.required, this.ageValidator]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^\(\d{3}\) \d{3}-\d{4}$/)]],

    // Step 3: Address
    street: ['', [Validators.required, Validators.minLength(5)]],
    city: ['', [Validators.required, Validators.minLength(2)]],
    state: ['', [Validators.required, Validators.minLength(2)]],
    zipCode: ['', [Validators.required, Validators.pattern(/^\d{5}$/)]]
  }, {
    validators: this.passwordMatchValidator
  });

  // Custom validator to check if passwords match
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      control.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  // Custom validator to check if user is at least 18 years old
  ageValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;

    const birthDate = new Date(control.value);
    const today = new Date();
    const age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      return age - 1 >= 18 ? null : { underAge: true };
    }

    return age >= 18 ? null : { underAge: true };
  }

  // Navigation between steps
  nextStep() {
    this.currentStep.update(step => Math.min(step + 1, 2));
  }

  previousStep() {
    this.currentStep.update(step => Math.max(step - 1, 0));
  }

  canProceedToStep2(): boolean {
    const step1Fields = ['firstName', 'lastName', 'email', 'password', 'confirmPassword'];
    return step1Fields.every(field => this.registerForm.get(field)?.valid);
  }

  canProceedToStep3(): boolean {
    const step2Fields = ['taxId', 'dateOfBirth', 'phoneNumber'];
    return step2Fields.every(field => this.registerForm.get(field)?.valid);
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    // Convert Date to ISO string for backend
    const dob = this.registerForm.value.dateOfBirth!;
    const dateOfBirthString = dob instanceof Date ? dob.toISOString() : String(dob);

    const request: RegisterRequest = {
      firstName: this.registerForm.value.firstName!,
      lastName: this.registerForm.value.lastName!,
      email: this.registerForm.value.email!,
      password: this.registerForm.value.password!,
      taxId: this.registerForm.value.taxId!,
      dateOfBirth: dateOfBirthString,
      phoneNumber: this.registerForm.value.phoneNumber!,
      street: this.registerForm.value.street!,
      city: this.registerForm.value.city!,
      state: this.registerForm.value.state!,
      zipCode: this.registerForm.value.zipCode!
    };

    this.api.register(request).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.successMessage.set('Registration successful! Redirecting to login...');
        console.log(response)
        // Redirect to login after 2 seconds
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 2000);
      },
      error: (err: any) => {
        this.isLoading.set(false);

        // Use enhanced error from error handling interceptor
        if (err.enhancedError) {
          this.errorMessage.set(err.enhancedError.message);
        } else {
          // Fallback for unexpected errors
          this.errorMessage.set('Registration failed. Please try again.');
        }
      }
    });
  }
}
