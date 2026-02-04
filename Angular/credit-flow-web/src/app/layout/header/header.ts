import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthStore } from '../../core/auth/auth.store';
import { Language } from '../../core/models/models';
import { Select } from 'primeng/select';
import { FormsModule } from '@angular/forms';

interface LanguageOption {
  label: string;
  value: Language;
  flag: string;
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [ButtonModule, Select, FormsModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  private router = inject(Router);
  authStore = inject(AuthStore);

  selectedLanguage = signal<Language>(Language.English);

  languageOptions: LanguageOption[] = [
    { label: 'English', value: Language.English, flag: 'united-kingdom.png' },
    { label: 'Ελληνικά', value: Language.Greek, flag: 'GreekFlag.png' }
  ];

  navigateToProfile() {
    this.router.navigate(['/profile']);
  }

  logout() {
    this.authStore.logout();
  }

  onLanguageChange(event: any) {
    this.selectedLanguage.set(event.value);
    // TODO: Emit or call LanguageService to handle translation changes
    console.log('Language changed to:', event.value);
  }
}
