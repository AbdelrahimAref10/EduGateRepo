import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PublicMarketplaceClient } from '../../core/api/academy-api.generated';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-public-lesson-redirect',
  standalone: true,
  imports: [TranslatePipe],
  template: `<p class="redirect">{{ 'marketplace.redirecting' | t }}</p>`,
  styles: `
    .redirect {
      padding: 8.5rem 1.5rem 5rem;
      text-align: center;
      color: #5a6b80;
      font-weight: 650;
    }
  `,
})
export class PublicLessonRedirectComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(PublicMarketplaceClient);
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    const lessonId = Number(this.route.snapshot.paramMap.get('lessonId'));
    if (!lessonId) {
      void this.router.navigateByUrl('/discover');
      return;
    }

    this.api.getLesson(lessonId).subscribe({
      next: (data) => {
        const teacherPath = this.auth.hasAnyRole(['Student'])
          ? ['/student/discover', data.teacherId]
          : ['/t', data.teacherId];
        void this.router.navigate(teacherPath, { queryParams: { lesson: data.lessonId } });
      },
      error: () => {
        void this.router.navigateByUrl('/discover');
      },
    });
  }
}
