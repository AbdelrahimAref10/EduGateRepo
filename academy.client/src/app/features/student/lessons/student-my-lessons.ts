import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  LessonsClient,
  StudentLessonListItemDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

@Component({
  selector: 'app-student-my-lessons',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink, UserAvatarComponent],
  templateUrl: './student-my-lessons.html',
  styleUrl: './student-my-lessons.css',
})
export class StudentMyLessonsComponent implements OnInit {
  private readonly lessonsApi = inject(LessonsClient);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly lessons = signal<StudentLessonListItemDto[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.lessonsApi.getMyLessons2().subscribe({
      next: (items) => {
        this.lessons.set(items ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load lessons.');
      },
    });
  }

  statusKey(status?: string): string {
    switch (status) {
      case 'Confirmed':
        return 'booking.statusConfirmed';
      case 'Rejected':
        return 'booking.statusRejected';
      default:
        return 'booking.statusPending';
    }
  }

  statusClass(status?: string): string {
    switch (status) {
      case 'Confirmed':
        return 'bg-emerald-50 text-emerald-700';
      case 'Rejected':
        return 'bg-rose-50 text-rose-700';
      default:
        return 'bg-amber-50 text-amber-800';
    }
  }
}
