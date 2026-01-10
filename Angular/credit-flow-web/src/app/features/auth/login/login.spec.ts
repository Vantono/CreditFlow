import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { of } from 'rxjs';

// Services
import { ApiService } from '../../../core/api/api.service';
import { AuthStore } from '../../../core/auth/auth.store';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  // 1. SIMPLE MOCKS (Χωρίς τη λέξη jasmine)
  // Ένα απλό αντικείμενο που έχει μια μέθοδο login και επιστρέφει Observable
  const apiServiceMock = {
    login: (req: any) => of({ 
      token: 'fake-jwt-token', 
      email: 'test@test.com', 
      userName: 'TestUser' 
    })
  };

  // Ένα απλό αντικείμενο για το Store
  const authStoreMock = {
    login: (data: any) => {}, // Κενή συνάρτηση, δεν κάνει τίποτα
    isAuthenticated: () => false // Επιστρέφει πάντα false
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideAnimationsAsync(),
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        
        // Σύνδεση των Mocks
        { provide: ApiService, useValue: apiServiceMock },
        { provide: AuthStore, useValue: authStoreMock }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should be invalid when empty', () => {
    // Εδώ ελέγχουμε απλά την τιμή του form, δεν χρειάζεται jasmine spy
    expect(component.loginForm.valid).toBeFalsy();
  });
});