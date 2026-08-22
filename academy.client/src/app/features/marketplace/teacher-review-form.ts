import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  StudentTeacherReviewsClient,
  TeacherReviewDto,
  UpsertTeacherReviewRequest,
} from '../../core/api/academy-api.generated';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-teacher-review-form',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './teacher-review-form.html',
})
export class TeacherReviewFormComponent implements OnChanges {
  private readonly reviewsApi = inject(StudentTeacherReviewsClient);

  @Input({ required: true }) teacherId = 0;
  @Input() review: TeacherReviewDto | null = null;
  @Output() saved = new EventEmitter<TeacherReviewDto>();

  readonly rating = signal(5);
  readonly comment = signal('');
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['review'] && !changes['teacherId']) return;
    this.rating.set(this.review?.rating || 5);
    this.comment.set(this.review?.comment ?? '');
    this.success.set(false);
    this.error.set(null);
  }

  setRating(value: number): void {
    this.rating.set(value);
  }

  submit(): void {
    if (!this.teacherId || this.saving()) return;

    this.saving.set(true);
    this.error.set(null);
    this.success.set(false);

    const body = new UpsertTeacherReviewRequest({
      rating: this.rating(),
      comment: this.comment().trim() || undefined,
    });

    const request$ = this.review
      ? this.reviewsApi.update(this.teacherId, body)
      : this.reviewsApi.create(this.teacherId, body);

    request$.subscribe({
      next: (saved) => {
        this.review = saved;
        this.saving.set(false);
        this.success.set(true);
        this.saved.emit(saved);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to save review.');
      },
    });
  }
}
