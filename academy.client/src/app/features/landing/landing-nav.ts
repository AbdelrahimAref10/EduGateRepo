import { Component, HostListener, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher';

@Component({
  selector: 'app-landing-nav',
  standalone: true,
  imports: [RouterLink, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './landing-nav.html',
  styleUrl: './landing-nav.css',
})
export class LandingNavComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly isAuthenticated = this.auth.isAuthenticated;
  readonly homeLink = () => this.auth.homeForCurrentUser();
  readonly navSolid = signal(false);
  readonly marketplaceOn = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
      startWith(this.router.url),
      map((url) => /^\/(discover|t\/|l\/)/.test(url)),
    ),
    { initialValue: /^\/(discover|t\/|l\/)/.test(this.router.url) },
  );

  constructor() {
    this.syncNav();
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.syncNav();
  }

  startNow(): void {
    if (this.isAuthenticated()) {
      void this.router.navigateByUrl(this.homeLink());
      return;
    }
    void this.router.navigate(['/register']);
  }

  private syncNav(): void {
    this.navSolid.set(typeof window !== 'undefined' && window.scrollY > 18);
  }
}
