import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LearningPathApi, WeeklyLearningPlanDto } from '../../core/api/learning-path-api.service';
import { ParentApi, ParentChildDto } from '../../core/api/parent-api.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { WeeklyPlanViewComponent } from '../learning/weekly-plan-view';

@Component({
  selector: 'app-parent-plan',
  standalone: true,
  imports: [TranslatePipe, FormsModule, WeeklyPlanViewComponent],
  template: `
    <section class="space-y-4">
      <header class="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 class="text-2xl font-bold text-ink">{{ 'learning.planTitle' | t }}</h1>
          <p class="text-sm text-muted">{{ 'learning.parentPlanSub' | t }}</p>
        </div>
        <label class="space-y-1 text-sm">
          <span class="text-xs font-semibold uppercase text-muted">{{ 'parent.filterChild' | t }}</span>
          <select
            class="block min-w-48 rounded-xl border border-line bg-surface px-3 py-2"
            [ngModel]="childFilter() ?? ''"
            (ngModelChange)="onFilterChange($event)"
          >
            <option value="">{{ 'parent.allChildren' | t }}</option>
            @for (c of children(); track c.childStudentId) {
              <option [value]="c.childStudentId">{{ c.fullName }}</option>
            }
          </select>
        </label>
      </header>
      @if (loading()) {
        <p class="text-sm text-muted">{{ 'common.loading' | t }}</p>
      } @else if (error()) {
        <p class="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{{ error() }}</p>
      } @else if (plan(); as p) {
        <app-weekly-plan-view [plan]="p" [showStudentName]="true" />
      }
    </section>
  `,
})
export class ParentPlanComponent implements OnInit {
  private readonly api = inject(LearningPathApi);
  private readonly parentApi = inject(ParentApi);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly plan = signal<WeeklyLearningPlanDto | null>(null);
  readonly children = signal<ParentChildDto[]>([]);
  readonly childFilter = signal<number | null>(null);

  ngOnInit(): void {
    this.parentApi.getChildren().subscribe({ next: (c) => this.children.set(c) });
    this.load();
  }

  onFilterChange(value: string): void {
    this.childFilter.set(value ? Number(value) : null);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getParentPlan(this.childFilter()).subscribe({
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
