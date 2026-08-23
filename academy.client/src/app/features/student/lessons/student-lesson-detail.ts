import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  BillingClient,
  LessonsClient,
  PaymentDto,
  StudentLessonDetailDto,
  StudentLessonSessionDto,
  StudentReviewsClient,
  StudentTeacherReviewsClient,
  TargetReviewDto,
  TeacherReviewDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { TeacherReviewFormComponent } from '../../marketplace/teacher-review-form';

@Component({
  selector: 'app-student-lesson-detail',
  standalone: true,
  imports: [TranslatePipe, DatePipe, DecimalPipe, RouterLink, TeacherReviewFormComponent, UserAvatarComponent],
  templateUrl: './student-lesson-detail.html',
  styleUrl: './student-lesson-detail.css',
})
export class StudentLessonDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly billingApi = inject(BillingClient);
  private readonly reviewsApi = inject(StudentTeacherReviewsClient);
  private readonly lessonReviewsApi = inject(StudentReviewsClient);

  readonly lessonId = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly detail = signal<StudentLessonDetailDto | null>(null);
  readonly canReview = signal(false);
  readonly myReview = signal<TeacherReviewDto | null>(null);
  readonly canReviewLesson = signal(false);
  readonly myLessonReview = signal<TargetReviewDto | null>(null);
  readonly payments = signal<PaymentDto[]>([]);
  readonly loadingPayments = signal(false);
  readonly downloadingPaymentId = signal<number | null>(null);

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
        this.loadReviews(id, data.teacherId, data.bookingStatus);
        this.loadPayments(id);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load lesson.');
      },
    });
  }

  loadPayments(lessonId = this.lessonId()): void {
    if (!lessonId) return;
    this.loadingPayments.set(true);
    this.billingApi.getMyPayments(lessonId).subscribe({
      next: (rows) => {
        this.payments.set(rows ?? []);
        this.loadingPayments.set(false);
      },
      error: () => {
        this.loadingPayments.set(false);
        this.payments.set([]);
      },
    });
  }

  downloadReceipt(payment: PaymentDto): void {
    if (!payment?.id) return;
    this.downloadingPaymentId.set(payment.id);
    this.billingApi.downloadReceipt2(payment.id).subscribe({
      next: (file) => {
        this.downloadingPaymentId.set(null);
        this.saveBlob(file.data, file.fileName || `receipt-${payment.receiptNumber}.pdf`);
      },
      error: (err) => {
        this.downloadingPaymentId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to download receipt.');
      },
    });
  }

  methodKey(method?: string | null): string {
    switch (method) {
      case 'Cash':
      case '1':
        return 'billing.methodCash';
      case 'VodafoneCash':
      case '2':
        return 'billing.methodVodafone';
      case 'InstaPay':
      case '3':
        return 'billing.methodInstaPay';
      default:
        return 'billing.methodOther';
    }
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

  private saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  private loadReviews(lessonId: number, teacherId?: number, bookingStatus?: string): void {
    if (bookingStatus !== 'Confirmed' || !teacherId) {
      this.canReview.set(false);
      this.myReview.set(null);
      this.canReviewLesson.set(false);
      this.myLessonReview.set(null);
      return;
    }

    forkJoin({
      teacher: this.reviewsApi.getMine(teacherId),
      lesson: this.lessonReviewsApi.getMyLessonReview(lessonId),
    }).subscribe({
      next: ({ teacher, lesson }) => {
        this.canReview.set(!!teacher.canReview);
        this.myReview.set(teacher.review ?? null);
        this.canReviewLesson.set(!!lesson.canReview);
        this.myLessonReview.set(lesson.review ?? null);
      },
      error: () => {
        this.canReview.set(true);
        this.canReviewLesson.set(true);
      },
    });
  }
}
