import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ParentApi, ParentAttendanceItemDto, ParentChildDto } from '../../core/api/parent-api.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { PaginatorComponent } from '../../shared/paginator/paginator';

@Component({
  selector: 'app-parent-attendance',
  standalone: true,
  imports: [TranslatePipe, DatePipe, FormsModule, PaginatorComponent],
  templateUrl: './parent-attendance.html',
})
export class ParentAttendanceComponent implements OnInit {
  private readonly api = inject(ParentApi);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly children = signal<ParentChildDto[]>([]);
  readonly items = signal<ParentAttendanceItemDto[]>([]);
  readonly page = signal(1);
  readonly pageSize = 20;
  readonly totalCount = signal(0);
  readonly childFilter = signal<number | null>(null);

  ngOnInit(): void {
    this.api.getChildren().subscribe({ next: (c) => this.children.set(c) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAttendance(this.childFilter(), this.page(), this.pageSize).subscribe({
      next: (data) => {
        this.items.set(data.items ?? []);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load attendance.');
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

  toTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
