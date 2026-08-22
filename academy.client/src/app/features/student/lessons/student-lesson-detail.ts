import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  LessonsClient,
  StudentLessonDetailDto,
  StudentLessonSessionDto,
  StudentTeacherReviewsClient,
  TeacherReviewDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { TeacherReviewFormComponent } from '../../marketplace/teacher-review-form';

@Component({
  selector: 'app-student-lesson-detail',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink, TeacherReviewFormComponent, UserAvatarComponent],
  templateUrl: './student-lesson-detail.html',
  styleUrl: './student-lesson-detail.css',
})
export class StudentLessonDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly reviewsApi = inject(StudentTeacherReviewsClient);

  readonly lessonId = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly detail = signal<StudentLessonDetailDto | null>(null);
  readonly canReview = signal(false);
  readonly myReview = signal<TeacherReviewDto | null>(null);

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
        this.loadReview(data.teacherId, data.bookingStatus);
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

  private loadReview(teacherId?: number, bookingStatus?: string): void {
    if (!teacherId || bookingStatus !== 'Confirmed') {
      this.canReview.set(false);
      this.myReview.set(null);
      return;
    }

    this.reviewsApi.getMine(teacherId).subscribe({
      next: (data) => {
        this.canReview.set(!!data.canReview);
        this.myReview.set(data.review ?? null);
      },
      error: () => {
        this.canReview.set(bookingStatus === 'Confirmed');
      },
    });
  }
}
