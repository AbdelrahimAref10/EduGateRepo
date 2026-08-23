import { Component, computed, input, output } from '@angular/core';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './paginator.html',
  styleUrl: './paginator.css',
})
export class PaginatorComponent {
  readonly page = input.required<number>();
  readonly pageSize = input(9);
  readonly totalCount = input.required<number>();
  readonly disabled = input(false);

  readonly pageChange = output<number>();

  readonly totalPages = computed(() => {
    const size = this.pageSize();
    const total = this.totalCount();
    if (size <= 0 || total <= 0) return 0;
    return Math.ceil(total / size);
  });

  readonly show = computed(() => this.totalPages() > 1);

  readonly rangeLabel = computed(() => {
    const total = this.totalCount();
    if (total <= 0) return '';
    const size = this.pageSize();
    const current = this.page();
    const from = (current - 1) * size + 1;
    const to = Math.min(current * size, total);
    return `${from}–${to}`;
  });

  readonly pages = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    if (total <= 1) return [] as number[];

    const window = 2;
    const start = Math.max(1, current - window);
    const end = Math.min(total, current + window);
    const list: number[] = [];

    if (start > 1) {
      list.push(1);
      if (start > 2) list.push(-1);
    }
    for (let i = start; i <= end; i++) list.push(i);
    if (end < total) {
      if (end < total - 1) list.push(-1);
      list.push(total);
    }
    return list;
  });

  go(page: number): void {
    if (this.disabled()) return;
    const total = this.totalPages();
    if (page < 1 || page > total || page === this.page()) return;
    this.pageChange.emit(page);
  }

  prev(): void {
    this.go(this.page() - 1);
  }

  next(): void {
    this.go(this.page() + 1);
  }
}
