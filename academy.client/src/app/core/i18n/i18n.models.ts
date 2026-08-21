export type AppLanguage = 'en' | 'ar';

/** Matches backend AppLanguage enum ids. */
export enum AppLanguageId {
  Arabic = 1,
  English = 2,
}

export interface LanguageOption {
  code: AppLanguage;
  id: AppLanguageId;
  label: string;
  nativeLabel: string;
  flag: string;
  dir: 'ltr' | 'rtl';
}

export const LANGUAGE_STORAGE_KEY = 'academy.ui.language';

export const LANGUAGES: LanguageOption[] = [
  { code: 'ar', id: AppLanguageId.Arabic, label: 'Arabic', nativeLabel: 'العربية', flag: '🇪🇬', dir: 'rtl' },
  { code: 'en', id: AppLanguageId.English, label: 'English', nativeLabel: 'English', flag: '🇬🇧', dir: 'ltr' },
];

export function languageCodeFromId(id: number | null | undefined): AppLanguage {
  return id === AppLanguageId.English ? 'en' : 'ar';
}

export function languageIdFromCode(code: AppLanguage): AppLanguageId {
  return code === 'en' ? AppLanguageId.English : AppLanguageId.Arabic;
}
