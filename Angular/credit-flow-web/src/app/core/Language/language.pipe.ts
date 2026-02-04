import { Pipe, PipeTransform } from '@angular/core';
import { LanguageService } from './language.service';

@Pipe({
  name: 'languagePipe',
  pure: false, // Σημαντικό: επιτρέπει στο pipe να ενημερώνεται άμεσα
  standalone: true
})
export class LanguagePipe implements PipeTransform {
  constructor(private langService: LanguageService) {}

  transform(key: string): string {
    return this.langService.translate(key);
  }
}