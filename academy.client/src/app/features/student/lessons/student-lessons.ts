import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  AvailableLessonDto,
  LessonsClient,
} from '../../../core/api/academy-api.generated';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-student-lessons',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink],
  templateUrl: './student-lessons.html',
  styleUrl: './student-lessons.css',
})
export class StudentLessonsComponent implements OnInit {
  private readonly lessonsApi = inject(LessonsClient);
  private readonly i18n = inject(TranslationService);

  readonly loading = signal(false);
  readonly bookingId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly lessons = signal<AvailableLessonDto[]>([]);
  readonly confirmLesson = signal<AvailableLessonDto | null>(null);

  ngOnInit(): void {
    this.loadLessons();
  }

  loadLessons(): void {
    this.loading.set(true);
    this.error.set(null);

    this.lessonsApi.getAvailableLessons().subscribe({
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

  openConfirm(lesson: AvailableLessonDto): void {
    if (this.bookingId() !== null) return;
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

    this.error.set(null);
    this.success.set(null);
    this.bookingId.set(lesson.id);

    this.lessonsApi.bookLesson(lesson.id).subscribe({
      next: () => {
        this.bookingId.set(null);
        this.confirmLesson.set(null);
        this.success.set('booked');
        this.lessons.update((items) => items.filter((item) => item.id !== lesson.id));
      },
      error: (err) => {
        this.bookingId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to book lesson.');
      },
    });
  }

  billingLabel(value?: string): string {
    if (value === 'Monthly') return 'lessons.monthly';
    return 'lessons.perSession';
  }

  price(lesson: AvailableLessonDto): number | string {
    if (lesson.billingType === 'Monthly') return lesson.monthlyPrice ?? '—';
    return lesson.sessionPrice ?? '—';
  }

  initials(name?: string): string {
    if (!name?.trim()) return '?';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  label(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
  }
}
