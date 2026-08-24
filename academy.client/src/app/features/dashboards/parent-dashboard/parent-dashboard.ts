import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ParentApi,
  ParentDashboardDto,
} from '../../../core/api/parent-api.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-parent-dashboard',
  standalone: true,
  imports: [TranslatePipe, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './parent-dashboard.html',
  styleUrl: './parent-dashboard.css',
})
export class ParentDashboardComponent implements OnInit {
  private readonly api = inject(ParentApi);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly data = signal<ParentDashboardDto | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getDashboard().subscribe({
      next: (d) => {
        this.data.set(d);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load dashboard.');
      },
    });
  }

  toTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
