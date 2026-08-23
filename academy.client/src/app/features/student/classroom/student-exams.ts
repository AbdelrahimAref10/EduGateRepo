import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { StudentExamListItemDto } from '../../../core/api/academy-api.generated';
import { DEFAULT_PAGE_SIZE } from '../../../core/api/paging';
import { StudentExamsApi } from '../../../core/api/student-exams-api.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PaginatorComponent } from '../../../shared/paginator/paginator';
import { StudentExamWorkspaceComponent } from './student-exam-workspace';

@Component({
  selector: 'app-student-exams',
  standalone: true,
  imports: [TranslatePipe, DatePipe, StudentExamWorkspaceComponent, PaginatorComponent],
  templateUrl: './student-exams.html',
  styleUrls: ['../../classroom/classroom-theme.css', './student-classroom-list.css'],
})
export class StudentExamsComponent implements OnInit {
  private readonly examsApi = inject(StudentExamsApi);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly items = signal<StudentExamListItemDto[]>([]);
  readonly page = signal(1);
  readonly pageSize = DEFAULT_PAGE_SIZE;
  readonly totalCount = signal(0);
  readonly openSessionId = signal<number | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(showSpinner = true): void {
    if (showSpinner) this.loading.set(true);
    this.error.set(null);
    this.examsApi.getMyExams(this.page(), this.pageSize).subscribe({
      next: (data) => {
        this.items.set(data.items ?? []);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.result?.detail || err?.message || 'Failed to load exams.');
      },
    });
  }

  onPageChange(page: number): void {
    if (page === this.page()) return;
    this.page.set(page);
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  openExam(item: StudentExamListItemDto): void {
    if (!item.canTake && !item.hasSubmitted) return;
    this.openSessionId.set(item.sessionId);
  }

  closeExam(): void {
    this.openSessionId.set(null);
    this.load(false);
  }

  toTimeInput(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  statusKey(item: StudentExamListItemDto): string {
    if (item.hasSubmitted) return 'myExams.done';
    if (item.hasStarted) return 'myExams.inProgress';
    if (item.sessionStarted) return 'myExams.ready';
    return 'myExams.waitingSession';
  }

  actionKey(item: StudentExamListItemDto): string {
    if (item.hasSubmitted) return 'classroom.viewResults';
    if (item.hasStarted) return 'classroom.continueExam';
    return 'classroom.startExam';
  }
}
