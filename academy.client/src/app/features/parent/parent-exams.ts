import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ParentApi, ParentChildDto, ParentExamListItemDto } from '../../core/api/parent-api.service';
import { StudentExamDto } from '../../core/api/academy-api.generated';
import { DEFAULT_PAGE_SIZE } from '../../core/api/paging';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { PaginatorComponent } from '../../shared/paginator/paginator';

@Component({
  selector: 'app-parent-exams',
  standalone: true,
  imports: [TranslatePipe, DatePipe, DecimalPipe, FormsModule, PaginatorComponent],
  templateUrl: './parent-exams.html',
})
export class ParentExamsComponent implements OnInit {
  private readonly api = inject(ParentApi);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly children = signal<ParentChildDto[]>([]);
  readonly items = signal<ParentExamListItemDto[]>([]);
  readonly page = signal(1);
  readonly pageSize = DEFAULT_PAGE_SIZE;
  readonly totalCount = signal(0);
  readonly childFilter = signal<number | null>(null);
  readonly detailLoading = signal(false);
  readonly detail = signal<StudentExamDto | null>(null);
  readonly detailTitle = signal('');

  ngOnInit(): void {
    this.api.getChildren().subscribe({ next: (c) => this.children.set(c) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getExams(this.childFilter(), this.page(), this.pageSize).subscribe({
      next: (data) => {
        this.items.set(data.items ?? []);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load exams.');
      },
    });
  }

  onFilterChange(value: string): void {
    this.childFilter.set(value ? Number(value) : null);
    this.page.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    if (page === this.page()) return;
    this.page.set(page);
    this.load();
  }

  openResult(item: ParentExamListItemDto): void {
    if (!item.hasSubmitted) return;
    this.detailLoading.set(true);
    this.detail.set(null);
    this.detailTitle.set(`${item.childName} · ${item.title}`);
    this.api.getChildExam(item.childStudentId, item.sessionId).subscribe({
      next: (exam) => {
        this.detail.set(exam);
        this.detailLoading.set(false);
      },
      error: (err) => {
        this.detailLoading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load exam.');
      },
    });
  }

  closeDetail(): void {
    this.detail.set(null);
    this.detailTitle.set('');
  }

  toTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
