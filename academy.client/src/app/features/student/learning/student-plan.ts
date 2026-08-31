import { Component, OnInit, inject, signal } from '@angular/core';
import { LearningPathApi, WeeklyLearningPlanDto } from '../../../core/api/learning-path-api.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { WeeklyPlanViewComponent } from '../../learning/weekly-plan-view';

@Component({
  selector: 'app-student-plan',
  standalone: true,
  imports: [TranslatePipe, WeeklyPlanViewComponent],
  template: `
    <section class="space-y-4">
      <header>
        <h1 class="text-2xl font-bold text-ink">{{ 'learning.planTitle' | t }}</h1>
        <p class="text-sm text-muted">{{ 'learning.planSub' | t }}</p>
      </header>
      @if (loading()) {
        <p class="text-sm text-muted">{{ 'common.loading' | t }}</p>
      } @else if (error()) {
        <p class="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{{ error() }}</p>
      } @else if (plan(); as p) {
        <app-weekly-plan-view [plan]="p" />
      }
    </section>
  `,
})
export class StudentPlanComponent implements OnInit {
  private readonly api = inject(LearningPathApi);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly plan = signal<WeeklyLearningPlanDto | null>(null);

  ngOnInit(): void {
    this.api.getStudentPlan().subscribe({
      next: (data) => {
        this.plan.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load plan.');
      },
    });
  }
}
