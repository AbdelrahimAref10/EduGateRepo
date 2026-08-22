import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  StudentReviewsClient,
  StudentTeacherReviewsClient,
  TargetReviewDto,
  TeacherReviewDto,
  UpsertReviewRequest,
  UpsertTeacherReviewRequest,
} from '../../core/api/academy-api.generated';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

export type ReviewFormKind = 'teacher' | 'lesson' | 'session';

@Component({
  selector: 'app-teacher-review-form',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './teacher-review-form.html',
})
export class TeacherReviewFormComponent implements OnChanges {
  private readonly teacherReviewsApi = inject(StudentTeacherReviewsClient);
  private readonly targetReviewsApi = inject(StudentReviewsClient);

  @Input() kind: ReviewFormKind = 'teacher';
  @Input() targetId = 0;
  @Input() teacherId = 0;
  @Input() review: TeacherReviewDto | TargetReviewDto | null = null;
  @Input() titleKey = 'marketplace.writeReview';
  @Input() editTitleKey = 'marketplace.editReview';
  @Input() hintKey = 'marketplace.reviewHint';
  @Output() saved = new EventEmitter<TeacherReviewDto | TargetReviewDto>();

  readonly rating = signal(5);
  readonly comment = signal('');
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal(false);

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['review'] && !changes['teacherId'] && !changes['targetId'] && !changes['kind']) return;
    this.rating.set(this.review?.rating || 5);
    this.comment.set(this.review?.comment ?? '');
    this.success.set(false);
    this.error.set(null);
  }

  headingKey(): string {
    return this.review ? this.editTitleKey : this.titleKey;
  }

  setRating(value: number): void {
    this.rating.set(value);
  }

  submit(): void {
    const id = this.kind === 'teacher' ? this.teacherId || this.targetId : this.targetId;
    if (!id || this.saving()) return;

    this.saving.set(true);
    this.error.set(null);
    this.success.set(false);

    if (this.kind === 'teacher') {
      const body = new UpsertTeacherReviewRequest({
        rating: this.rating(),
        comment: this.comment().trim() || undefined,
      });
      const request$ = this.review
        ? this.teacherReviewsApi.update(id, body)
        : this.teacherReviewsApi.create(id, body);
      request$.subscribe({
        next: (saved) => this.onSaved(saved),
        error: (err) => this.onError(err),
      });
      return;
    }

    const body = new UpsertReviewRequest({
      rating: this.rating(),
      comment: this.comment().trim() || undefined,
    });
    const request$ = this.kind === 'lesson'
      ? this.targetReviewsApi.upsertLessonReview(id, body)
      : this.targetReviewsApi.upsertSessionReview(id, body);
    request$.subscribe({
      next: (saved) => this.onSaved(saved),
      error: (err) => this.onError(err),
    });
  }

  private onSaved(saved: TeacherReviewDto | TargetReviewDto): void {
    this.review = saved;
    this.saving.set(false);
    this.success.set(true);
    this.saved.emit(saved);
  }

  private onError(err: { result?: { detail?: string }; message?: string }): void {
    this.saving.set(false);
    this.error.set(err?.result?.detail || err?.message || 'Failed to save review.');
  }
}
