import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  StudentClient,
  TeacherBookingDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

type Tab = 'pending' | 'all';

@Component({
  selector: 'app-teacher-bookings',
  standalone: true,
  imports: [TranslatePipe, DatePipe],
  templateUrl: './teacher-bookings.html',
  styleUrl: './teacher-bookings.css',
})
export class TeacherBookingsComponent implements OnInit {
  private readonly bookingsApi = inject(StudentClient);

  readonly loading = signal(false);
  readonly actionId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly tab = signal<Tab>('pending');
  readonly pending = signal<TeacherBookingDto[]>([]);
  readonly all = signal<TeacherBookingDto[]>([]);

  readonly rows = computed(() => (this.tab() === 'pending' ? this.pending() : this.all()));

  ngOnInit(): void {
    this.reload();
  }

  setTab(value: Tab): void {
    this.tab.set(value);
    this.error.set(null);
    this.success.set(null);
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingsApi.getPendingBookings().subscribe({
      next: (items) => {
        this.pending.set(items ?? []);
        this.bookingsApi.getAllBookings().subscribe({
          next: (allItems) => {
            this.all.set(allItems ?? []);
            this.loading.set(false);
          },
          error: (err) => {
            this.loading.set(false);
            this.error.set(err?.result?.detail || err?.message || 'Failed to load bookings.');
          },
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load bookings.');
      },
    });
  }

  confirm(booking: TeacherBookingDto): void {
    if (!booking.id || this.actionId() !== null) return;

    this.error.set(null);
    this.success.set(null);
    this.actionId.set(booking.id);

    this.bookingsApi.confirmBooking(booking.id).subscribe({
      next: (updated) => {
        this.actionId.set(null);
        this.success.set('confirmed');
        this.applyUpdate(updated);
      },
      error: (err) => {
        this.actionId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to confirm booking.');
      },
    });
  }

  reject(booking: TeacherBookingDto): void {
    if (!booking.id || this.actionId() !== null) return;

    this.error.set(null);
    this.success.set(null);
    this.actionId.set(booking.id);

    this.bookingsApi.rejectBooking(booking.id).subscribe({
      next: (updated) => {
        this.actionId.set(null);
        this.success.set('rejected');
        this.applyUpdate(updated);
      },
      error: (err) => {
        this.actionId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to reject booking.');
      },
    });
  }

  statusLabel(status?: string): string {
    switch (status) {
      case 'Confirmed':
        return 'booking.statusConfirmed';
      case 'Rejected':
        return 'booking.statusRejected';
      default:
        return 'booking.statusPending';
    }
  }

  private applyUpdate(updated: TeacherBookingDto): void {
    this.pending.update((items) => items.filter((item) => item.id !== updated.id));
    this.all.update((items) => {
      const next = items.filter((item) => item.id !== updated.id);
      return [updated, ...next];
    });
  }
}
