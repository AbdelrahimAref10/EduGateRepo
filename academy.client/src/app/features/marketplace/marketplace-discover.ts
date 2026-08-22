import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  CountriesClient,
  EducationTypesClient,
  PublicMarketplaceClient,
} from '../../core/api/academy-api.generated';
import { AuthService } from '../../core/auth/auth.service';
import { TranslationService } from '../../core/i18n/translation.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../shared/user-avatar/user-avatar';
import { MarketplaceCatalog } from './marketplace-catalog';
import { filledStars, ratingLabel } from './marketplace.util';
import { RatingStarsComponent } from './rating-stars';

@Component({
  selector: 'app-marketplace-discover',
  standalone: true,
  imports: [TranslatePipe, RatingStarsComponent, UserAvatarComponent],
  templateUrl: './marketplace-discover.html',
  styleUrl: './marketplace-discover.css',
})
export class MarketplaceDiscoverComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly catalog = new MarketplaceCatalog(
    inject(PublicMarketplaceClient),
    inject(CountriesClient),
    inject(EducationTypesClient),
    inject(TranslationService),
  );

  readonly marquee = [
    'landing.band1',
    'landing.band2',
    'landing.band3',
    'landing.band4',
    'landing.band5',
    'landing.band6',
  ];

  ngOnInit(): void {
    this.catalog.init();
  }

  rating(average?: number, count?: number): string {
    return ratingLabel(average, count);
  }

  stars(average?: number, count?: number, value?: number): number {
    return filledStars(average, count, value);
  }

  openTeacher(teacherId: number): void {
    if (this.auth.hasAnyRole(['Student'])) {
      void this.router.navigate(['/student/discover', teacherId]);
      return;
    }
    void this.router.navigate(['/t', teacherId]);
  }

  startNow(): void {
    if (this.auth.isAuthenticated()) {
      void this.router.navigateByUrl(this.auth.homeForCurrentUser());
      return;
    }
    void this.router.navigate(['/register']);
  }

  hideBrokenImage(event: Event): void {
    (event.target as HTMLImageElement).style.opacity = '0';
  }
}
