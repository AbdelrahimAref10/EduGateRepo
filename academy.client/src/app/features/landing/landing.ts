import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { LandingFooterComponent } from './landing-footer';
import { LandingNavComponent } from './landing-nav';

interface HeroSlide {
  image: string;
  kicker: string;
  title: string;
  body: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, TranslatePipe, LandingNavComponent, LandingFooterComponent],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class LandingComponent implements OnInit, AfterViewInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly destroyRef = inject(DestroyRef);

  readonly isAuthenticated = this.auth.isAuthenticated;
  readonly homeLink = () => this.auth.homeForCurrentUser();
  readonly heroIndex = signal(0);
  readonly quotePage = signal(0);

  readonly slides: HeroSlide[] = [
    {
      image: 'assets/images/landing/hero-1.jpg',
      kicker: 'landing.slide1Kicker',
      title: 'landing.slide1Title',
      body: 'landing.slide1Body',
    },
    {
      image: 'assets/images/landing/hero-2.jpg',
      kicker: 'landing.slide2Kicker',
      title: 'landing.slide2Title',
      body: 'landing.slide2Body',
    },
    {
      image: 'assets/images/landing/hero-3.jpg',
      kicker: 'landing.slide3Kicker',
      title: 'landing.slide3Title',
      body: 'landing.slide3Body',
    },
  ];

  readonly quotes = [
    { name: 'landing.q1Name', role: 'landing.q1Role', text: 'landing.q1Text' },
    { name: 'landing.q2Name', role: 'landing.q2Role', text: 'landing.q2Text' },
    { name: 'landing.q3Name', role: 'landing.q3Role', text: 'landing.q3Text' },
    { name: 'landing.q4Name', role: 'landing.q4Role', text: 'landing.q4Text' },
  ];

  readonly quotePageCount = computed(() => Math.ceil(this.quotes.length / 2));

  readonly visibleQuotes = computed(() => {
    const start = this.quotePage() * 2;
    return this.quotes.slice(start, start + 2);
  });

  readonly marquee = [
    'landing.band1',
    'landing.band2',
    'landing.band3',
    'landing.band4',
    'landing.band5',
    'landing.band6',
  ];

  private heroTimer?: ReturnType<typeof setInterval>;
  private quoteTimer?: ReturnType<typeof setInterval>;
  private revealObs?: IntersectionObserver;
  private previousHtmlScroll = '';

  ngOnInit(): void {
    this.previousHtmlScroll = document.documentElement.style.scrollBehavior;
    document.documentElement.style.scrollBehavior = 'smooth';
    this.restartHeroTimer();
    this.quoteTimer = setInterval(() => this.nextQuote(), 6200);
  }

  ngAfterViewInit(): void {
    this.revealObs = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) continue;
          entry.target.classList.add('is-in');
          this.revealObs?.unobserve(entry.target);
        }
      },
      { threshold: 0.18, rootMargin: '0px 0px -12% 0px' },
    );

    this.host.nativeElement.querySelectorAll('.reveal').forEach((el: Element) => {
      this.revealObs?.observe(el);
    });

    this.route.fragment.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((id) => {
      if (!id) return;
      setTimeout(() => {
        document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }, 60);
    });

    this.destroyRef.onDestroy(() => this.revealObs?.disconnect());
  }

  ngOnDestroy(): void {
    if (this.heroTimer) clearInterval(this.heroTimer);
    if (this.quoteTimer) clearInterval(this.quoteTimer);
    document.documentElement.style.scrollBehavior = this.previousHtmlScroll;
  }

  startNow(): void {
    if (this.isAuthenticated()) {
      void this.router.navigateByUrl(this.homeLink());
      return;
    }
    void this.router.navigate(['/register']);
  }

  nextHero(): void {
    this.heroIndex.update((i) => (i + 1) % this.slides.length);
    this.restartHeroTimer();
  }

  prevHero(): void {
    this.heroIndex.update((i) => (i - 1 + this.slides.length) % this.slides.length);
    this.restartHeroTimer();
  }

  goHero(index: number): void {
    this.heroIndex.set(index);
    this.restartHeroTimer();
  }

  nextQuote(): void {
    const last = this.quotePageCount();
    if (last < 2) return;
    this.quotePage.update((i) => (i + 1) % last);
  }

  prevQuote(): void {
    const last = this.quotePageCount();
    if (last < 2) return;
    this.quotePage.update((i) => (i - 1 + last) % last);
  }

  goQuote(page: number): void {
    this.quotePage.set(page);
  }

  hideBrokenImage(event: Event): void {
    (event.target as HTMLImageElement).style.opacity = '0';
  }

  private restartHeroTimer(): void {
    if (this.heroTimer) clearInterval(this.heroTimer);
    this.heroTimer = setInterval(() => {
      this.heroIndex.update((i) => (i + 1) % this.slides.length);
    }, 7200);
  }
}
