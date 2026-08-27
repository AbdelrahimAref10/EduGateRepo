import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  AcademicYearsClient,
  CountriesClient,
  EducationStagesClient,
  PublicMarketplaceClient,
  PublicTeacherListItemDto,
} from '../../../core/api/academy-api.generated';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { MarketplaceCatalog } from '../../marketplace/marketplace-catalog';
import { filledStars, ratingLabel } from '../../marketplace/marketplace.util';
import { RatingStarsComponent } from '../../marketplace/rating-stars';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

@Component({
  selector: 'app-student-discover',
  standalone: true,
  imports: [RouterLink, TranslatePipe, RatingStarsComponent, UserAvatarComponent],
  templateUrl: './student-discover.html',
  styleUrl: './student-discover.css',
})
export class StudentDiscoverComponent implements OnInit {
  private readonly router = inject(Router);
  readonly catalog = new MarketplaceCatalog(
    inject(PublicMarketplaceClient),
    inject(CountriesClient),
    inject(AcademicYearsClient),
    inject(EducationStagesClient),
    inject(TranslationService),
  );

  ngOnInit(): void {
    this.catalog.init();
  }

  rating(teacher: PublicTeacherListItemDto): string {
    return ratingLabel(teacher.ratingAverage, teacher.ratingCount);
  }

  stars(teacher: PublicTeacherListItemDto): number {
    return filledStars(teacher.ratingAverage, teacher.ratingCount, teacher.ratingStars);
  }

  open(teacher: PublicTeacherListItemDto): void {
    void this.router.navigate(['/student/discover', teacher.id]);
  }
}
