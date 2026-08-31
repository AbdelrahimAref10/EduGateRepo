import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { WeeklyLearningPlanDto } from '../../core/api/learning-path-api.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-weekly-plan-view',
  standalone: true,
  imports: [TranslatePipe, DatePipe, DecimalPipe],
  templateUrl: './weekly-plan-view.html',
})
export class WeeklyPlanViewComponent {
  readonly plan = input.required<WeeklyLearningPlanDto>();
  readonly showStudentName = input(false);

  toTime(value?: string | null): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
