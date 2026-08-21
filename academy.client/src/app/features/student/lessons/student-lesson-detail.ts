import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  LessonsClient,
  StudentLessonDetailDto,
  StudentLessonSessionDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-student-lesson-detail',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink],
  templateUrl: './student-lesson-detail.html',
  styleUrl: './student-lesson-detail.css',
})
export class StudentLessonDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly lessonsApi = inject(LessonsClient);

  readonly lessonId = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly detail = signal<StudentLessonDetailDto | null>(null);

  ngOnInit(): void {
    this.lessonId.set(Number(this.route.snapshot.paramMap.get('lessonId')));
    this.load();
  }

  load(): void {
    const id = this.lessonId();
    if (!id) {
      this.error.set('Lesson not found.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.lessonsApi.getMyLessonDetail(id).subscribe({
      next: (data) => {
        this.detail.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load lesson.');
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

  sessionStatusKey(session: StudentLessonSessionDto): string {
    if (session.hasEnded) return 'studentLessons.sessionEnded';
    if (session.hasStarted) return 'studentLessons.sessionLive';
    return 'studentLessons.sessionPending';
  }

  toTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
