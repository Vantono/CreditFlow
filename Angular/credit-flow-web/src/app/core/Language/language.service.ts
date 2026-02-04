import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  // Ορίζουμε τα λεκτικά μας
  private data: any = {
    'GR': {
      'accept': 'Αποδοχή',
      'cancel': 'Ακύρωση'
    },
    'EN': {
      'accept': 'Accept',
      'cancel': 'Cancel'
    }
  };

  // Το flag που "ακούει" όλη η εφαρμογή (default 'GR')
  private languageFlag = new BehaviorSubject<string>('GR');
  language$ = this.languageFlag.asObservable();

  // Μέθοδος για να αλλάζουμε γλώσσα από το header
  setLanguage(lang: string) {
    this.languageFlag.next(lang);
  }

  // Μέθοδος για να παίρνουμε το τρέχον λεκτικό
  translate(key: string): string {
    const currentLang = this.languageFlag.value;
    return this.data[currentLang][key] || key;
  }
}