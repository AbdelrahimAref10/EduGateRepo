import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { LessonProgressDto } from '../../core/api/learning-path-api.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-progress-report-view',
  standalone: true,
  imports: [TranslatePipe, DatePipe, DecimalPipe],
  templateUrl: './progress-report-view.html',
})
export class ProgressReportViewComponent {
  readonly lessons = input.required<LessonProgressDto[]>();
  readonly showStudentName = input(false);

  toTime(value?: string | null): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
