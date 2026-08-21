import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationService } from './translation.service';

@Pipe({
  name: 't',
  standalone: true,
  pure: false,
})
export class TranslatePipe implements PipeTransform {
  private readonly i18n = inject(TranslationService);

  transform(key: string, fallback?: string): string {
    // Depend on language signal so the impure pipe refreshes on language change.
    this.i18n.language();
    return this.i18n.t(key, fallback);
  }
}
