import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  AppLanguage,
  LANGUAGE_STORAGE_KEY,
  LANGUAGES,
  LanguageOption,
  languageCodeFromId,
} from './i18n.models';

type Dictionary = Record<string, unknown>;

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private readonly http = inject(HttpClient);

  private readonly languageSignal = signal<AppLanguage>(this.readStoredLanguage());
  private readonly dictionarySignal = signal<Dictionary>({});
  private readonly readySignal = signal(false);

  readonly language = this.languageSignal.asReadonly();
  readonly ready = this.readySignal.asReadonly();
  readonly languages = LANGUAGES;
  readonly currentLanguage = computed(
    () => LANGUAGES.find((item) => item.code === this.languageSignal()) ?? LANGUAGES[0],
  );
  readonly isRtl = computed(() => this.currentLanguage().dir === 'rtl');

  async init(): Promise<void> {
    await this.loadLanguage(this.languageSignal());
  }

  async setLanguage(code: AppLanguage): Promise<void> {
    if (code === this.languageSignal() && this.readySignal()) {
      return;
    }

    localStorage.setItem(LANGUAGE_STORAGE_KEY, code);
    await this.loadLanguage(code);
  }

  /** Sync UI language from auth session PreferredLanguage id. */
  async syncFromLanguageId(languageId: number | null | undefined): Promise<void> {
    await this.setLanguage(languageCodeFromId(languageId));
  }

  t(key: string, fallback = key): string {
    const value = this.resolve(key, this.dictionarySignal());
    return typeof value === 'string' ? value : fallback;
  }

  private async loadLanguage(code: AppLanguage): Promise<void> {
    const dict = await firstValueFrom(
      this.http.get<Dictionary>(`/assets/i18n/${code}.json`),
    );

    this.dictionarySignal.set(dict);
    this.languageSignal.set(code);
    this.readySignal.set(true);
    this.applyDocumentDirection(code);
  }

  private applyDocumentDirection(code: AppLanguage): void {
    const option = LANGUAGES.find((item) => item.code === code) as LanguageOption;
    document.documentElement.lang = code;
    document.documentElement.dir = option.dir;
  }

  private readStoredLanguage(): AppLanguage {
    const stored = localStorage.getItem(LANGUAGE_STORAGE_KEY);
    if (stored && LANGUAGES.some((item) => item.code === stored)) {
      return stored as AppLanguage;
    }
    return 'ar';
  }

  private resolve(path: string, source: Dictionary): unknown {
    return path.split('.').reduce<unknown>((acc, part) => {
      if (acc && typeof acc === 'object' && part in (acc as Dictionary)) {
        return (acc as Dictionary)[part];
      }
      return undefined;
    }, source);
  }
}
