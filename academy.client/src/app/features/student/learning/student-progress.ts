import { Component, OnInit, inject, signal } from '@angular/core';
import { LearningPathApi, LessonProgressDto } from '../../../core/api/learning-path-api.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { ProgressReportViewComponent } from '../../learning/progress-report-view';

@Component({
  selector: 'app-student-progress',
  standalone: true,
  imports: [TranslatePipe, ProgressReportViewComponent],
  template: `
    <section class="space-y-4">
      <header>
        <h1 class="text-2xl font-bold text-ink">{{ 'learning.progressTitle' | t }}</h1>
        <p class="text-sm text-muted">{{ 'learning.progressSub' | t }}</p>
      </header>
      @if (loading()) {
        <p class="text-sm text-muted">{{ 'common.loading' | t }}</p>
      } @else if (error()) {
        <p class="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{{ error() }}</p>
      } @else {
        <app-progress-report-view [lessons]="lessons()" />
      }
    </section>
  `,
})
export class StudentProgressComponent implements OnInit {
  private readonly api = inject(LearningPathApi);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly lessons = signal<LessonProgressDto[]>([]);

  ngOnInit(): void {
    this.api.getStudentProgress().subscribe({
      next: (data) => {
        this.lessons.set(data.lessons ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load progress.');
      },
    });
  }
}
