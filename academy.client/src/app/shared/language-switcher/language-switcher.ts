import { Component, HostListener, Input, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TranslationService } from '../../core/i18n/translation.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { AppLanguage } from '../../core/i18n/i18n.models';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.css',
})
export class LanguageSwitcherComponent {
  @Input() tone: 'default' | 'on-primary' = 'default';

  private readonly i18n = inject(TranslationService);
  private readonly auth = inject(AuthService);

  readonly open = signal(false);
  readonly busy = signal(false);
  readonly languages = this.i18n.languages;
  readonly current = this.i18n.currentLanguage;

  toggle(event: MouseEvent): void {
    event.stopPropagation();
    this.open.update((value) => !value);
  }

  async select(code: AppLanguage, event: MouseEvent): Promise<void> {
    event.stopPropagation();
    if (this.busy() || code === this.i18n.language()) {
      this.open.set(false);
      return;
    }

    this.busy.set(true);
    try {
      if (this.auth.isAuthenticated()) {
        await firstValueFrom(this.auth.updatePreferredLanguage(code));
        window.location.reload();
        return;
      }

      await this.i18n.setLanguage(code);
      this.open.set(false);
    } catch {
      this.open.set(false);
    } finally {
      this.busy.set(false);
    }
  }

  @HostListener('document:click')
  close(): void {
    this.open.set(false);
  }
}
