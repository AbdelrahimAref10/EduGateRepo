import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  LessonsClient,
  PublicLessonCardDto,
  PublicMarketplaceClient,
  PublicTeacherDetailDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { TeacherReviewFormComponent } from '../../marketplace/teacher-review-form';
import { filledStars, ratingLabel, resolvedRating } from '../../marketplace/marketplace.util';
import { RatingStarsComponent } from '../../marketplace/rating-stars';

@Component({
  selector: 'app-student-teacher',
  standalone: true,
  imports: [DatePipe, RouterLink, TranslatePipe, TeacherReviewFormComponent, RatingStarsComponent, UserAvatarComponent],
  templateUrl: './student-teacher.html',
  styleUrl: './student-teacher.css',
})
export class StudentTeacherComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly marketplaceApi = inject(PublicMarketplaceClient);
  private readonly lessonsApi = inject(LessonsClient);

  readonly teacherId = signal(0);
  readonly highlightLessonId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly bookingId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly teacher = signal<PublicTeacherDetailDto | null>(null);
  readonly confirmLesson = signal<PublicLessonCardDto | null>(null);

  ngOnInit(): void {
    this.teacherId.set(Number(this.route.snapshot.paramMap.get('teacherId')));
    const lesson = Number(this.route.snapshot.queryParamMap.get('lesson'));
    this.highlightLessonId.set(Number.isFinite(lesson) && lesson > 0 ? lesson : null);
    this.load();
  }

  load(): void {
    const id = this.teacherId();
    if (!id) {
      this.error.set('Teacher not found.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.marketplaceApi.getTeacher(id).subscribe({
      next: (data) => {
        this.teacher.set(data);
        this.loading.set(false);
        const lessonId = this.highlightLessonId();
        if (lessonId) {
          queueMicrotask(() =>
            document.getElementById(`lesson-${lessonId}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' }),
          );
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load teacher.');
      },
    });
  }

  ratingText(teacher: PublicTeacherDetailDto): string {
    return ratingLabel(teacher.ratingAverage, teacher.ratingCount, teacher.reviews);
  }

  ratingCount(teacher: PublicTeacherDetailDto): number {
    return resolvedRating(teacher.ratingAverage, teacher.ratingCount, teacher.reviews).count;
  }

  stars(teacher: PublicTeacherDetailDto): number {
    return filledStars(teacher.ratingAverage, teacher.ratingCount, teacher.ratingStars, teacher.reviews);
  }

  seatsKey(lesson: PublicLessonCardDto): string {
    if (lesson.seatsOpen) return 'marketplace.seatsOpen';
    if (lesson.isFull) return 'marketplace.seatsFull';
    return 'marketplace.seatsLeft';
  }

  billingLabel(value?: string): string {
    if (value === 'Monthly') return 'lessons.monthly';
    return 'lessons.perSession';
  }

  price(lesson: PublicLessonCardDto): number | string {
    if (lesson.billingType === 'Monthly') return lesson.monthlyPrice ?? '—';
    return lesson.sessionPrice ?? '—';
  }

  canBook(lesson: PublicLessonCardDto): boolean {
    return !lesson.isFull && !lesson.alreadyBooked && !this.teacher()?.isOwnProfile;
  }

  openBook(lesson: PublicLessonCardDto): void {
    if (!this.canBook(lesson) || this.bookingId() !== null) return;
    this.error.set(null);
    this.confirmLesson.set(lesson);
  }

  closeConfirm(): void {
    if (this.bookingId() !== null) return;
    this.confirmLesson.set(null);
  }

  confirmBook(): void {
    const lesson = this.confirmLesson();
    if (!lesson?.id || this.bookingId() !== null) return;

    this.bookingId.set(lesson.id);
    this.error.set(null);
    this.lessonsApi.bookLesson(lesson.id).subscribe({
      next: () => {
        this.bookingId.set(null);
        this.confirmLesson.set(null);
        this.success.set('booked');
        this.load();
      },
      error: (err) => {
        this.bookingId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to book lesson.');
      },
    });
  }
}
