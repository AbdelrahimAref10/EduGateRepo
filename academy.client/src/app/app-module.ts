import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { APP_INITIALIZER, NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { authInterceptor } from './core/auth/auth.interceptor';
import { TokenStorageService } from './core/auth/token-storage.service';
import { TranslationService } from './core/i18n/translation.service';
import { LANGUAGE_STORAGE_KEY, languageCodeFromId } from './core/i18n/i18n.models';

export function initTranslations(i18n: TranslationService, tokens: TokenStorageService) {
  return async () => {
    const session = tokens.getSession();
    if (session?.languageId) {
      localStorage.setItem(LANGUAGE_STORAGE_KEY, languageCodeFromId(session.languageId));
    }
    await i18n.init();
  };
}

@NgModule({
  declarations: [App],
  imports: [BrowserModule, AppRoutingModule],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: initTranslations,
      deps: [TranslationService, TokenStorageService],
      multi: true,
    },
  ],
  bootstrap: [App],
})
export class AppModule {}
