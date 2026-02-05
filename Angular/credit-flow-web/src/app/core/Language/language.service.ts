import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { EN_TRANSLATIONS } from './translations.en';
import { GR_TRANSLATIONS } from './translations.gr';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  // Ορίζουμε τα λεκτικά μας
  private data: Record<string, Record<string, string>> = {
    GR: GR_TRANSLATIONS,
    EN: EN_TRANSLATIONS
  };

  private languageFlag = new BehaviorSubject<string>('EN');
  language$ = this.languageFlag.asObservable();

  // Μέθοδος για να αλλάζουμε γλώσσα από το header
  setLanguage(lang: string) {
    this.languageFlag.next(lang);
  }

  translate(key: string): string {
    const currentLang = this.languageFlag.value;
    return this.data[currentLang]?.[key] || key;
  }
}