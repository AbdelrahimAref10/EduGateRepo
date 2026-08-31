import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LearningPathApi, LessonProgressDto, WeeklyLearningPlanDto } from '../../../core/api/learning-path-api.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { WeeklyPlanViewComponent } from '../../learning/weekly-plan-view';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [TranslatePipe, RouterLink, WeeklyPlanViewComponent],
  template: `
    <section class="space-y-6" data-accent="student">
      <header class="space-y-1">
        <p class="text-xs font-semibold uppercase tracking-wide text-primary-600">{{ 'auth.roleStudent' | t }}</p>
        <h1 class="text-2xl font-bold text-ink md:text-3xl">{{ 'dashboard.studentTitle' | t }}</h1>
        <p class="text-sm text-muted">{{ 'dashboard.studentSub' | t }}</p>
      </header>

      @if (loading()) {
        <p class="text-sm text-muted">{{ 'common.loading' | t }}</p>
      } @else if (error()) {
        <p class="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{{ error() }}</p>
      } @else {
        <div class="grid gap-4 md:grid-cols-3">
          <a routerLink="/student/plan" class="rounded-2xl border border-line bg-surface p-4 transition hover:border-primary-300">
            <p class="text-xs font-semibold uppercase text-muted">{{ 'learning.planNav' | t }}</p>
            <p class="mt-2 text-3xl font-bold text-ink">{{ plan()?.sessions?.length ?? 0 }}</p>
            <p class="mt-1 text-sm text-muted">{{ 'learning.thisWeekSessions' | t }}</p>
          </a>
          <a routerLink="/student/progress" class="rounded-2xl border border-line bg-surface p-4 transition hover:border-primary-300">
            <p class="text-xs font-semibold uppercase text-muted">{{ 'learning.progressNav' | t }}</p>
            <p class="mt-2 text-3xl font-bold text-ink">{{ lessons().length }}</p>
            <p class="mt-1 text-sm text-muted">{{ 'learning.lessonsCount' | t }}</p>
          </a>
          <a routerLink="/student/exams" class="rounded-2xl border border-line bg-surface p-4 transition hover:border-primary-300">
            <p class="text-xs font-semibold uppercase text-muted">{{ 'learning.examsDue' | t }}</p>
            <p class="mt-2 text-3xl font-bold text-ink">{{ plan()?.examsDue?.length ?? 0 }}</p>
          </a>
        </div>

        @if (plan(); as p) {
          <app-weekly-plan-view [plan]="p" />
        }
      }
    </section>
  `,
})
export class StudentDashboardComponent implements OnInit {
  private readonly api = inject(LearningPathApi);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly plan = signal<WeeklyLearningPlanDto | null>(null);
  readonly lessons = signal<LessonProgressDto[]>([]);

  ngOnInit(): void {
    forkJoin({
      plan: this.api.getStudentPlan(),
      progress: this.api.getStudentProgress(),
    }).subscribe({
      next: ({ plan, progress }) => {
        this.plan.set(plan);
        this.lessons.set(progress.lessons ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load dashboard.');
      },
    });
  }
}
