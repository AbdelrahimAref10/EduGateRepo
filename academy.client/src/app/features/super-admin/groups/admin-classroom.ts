import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  AdminClassroomDto,
  AdminReviewsDto,
  ClassroomMaterialDto,
  LessonsOverviewClient,
  SuperAdminSessionsClient,
  TeacherExamResultsDto,
  TeacherStudentExamReviewDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { RatingStarsComponent } from '../../marketplace/rating-stars';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';

type ClassroomTab = 'students' | 'materials' | 'reviews' | 'exam';
type ReviewKind = 'session' | 'lesson';

const CLASSROOM_TABS: { id: ClassroomTab; key: string }[] = [
  { id: 'students', key: 'adminGroups.studentsTab' },
  { id: 'materials', key: 'adminGroups.materialsTab' },
  { id: 'reviews', key: 'adminGroups.reviews' },
  { id: 'exam', key: 'adminGroups.exam' },
];

@Component({
  selector: 'app-admin-classroom',
  standalone: true,
  imports: [DatePipe, RouterLink, TranslatePipe, RatingStarsComponent, UserAvatarComponent, PageLoaderComponent],
  templateUrl: './admin-classroom.html',
  styleUrl: './admin-classroom.css',
})
export class AdminClassroomComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly sessionsApi = inject(SuperAdminSessionsClient);
  private readonly overviewApi = inject(LessonsOverviewClient);
  private readonly destroyRef = inject(DestroyRef);

  readonly sessionId = signal(0);
  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly error = signal<string | null>(null);
  readonly classroom = signal<AdminClassroomDto | null>(null);
  readonly tab = signal<ClassroomTab>('students');

  readonly reviewKind = signal<ReviewKind>('session');
  readonly sessionReviews = signal<AdminReviewsDto | null>(null);
  readonly lessonReviews = signal<AdminReviewsDto | null>(null);
  readonly loadingReviews = signal(false);

  readonly examResults = signal<TeacherExamResultsDto | null>(null);
  readonly loadingExam = signal(false);
  readonly studentReviews = signal<Record<number, TeacherStudentExamReviewDto>>({});
  readonly loadingStudentId = signal<number | null>(null);
  readonly openStudentId = signal<number | null>(null);

  readonly downloadingMaterialId = signal<number | null>(null);
  readonly tabs = CLASSROOM_TABS;

  readonly presentCount = computed(
    () => (this.classroom()?.students ?? []).filter((x) => x.isPresent).length,
  );
  readonly paidCount = computed(
    () => (this.classroom()?.students ?? []).filter((x) => x.isPaid).length,
  );
  readonly openReview = computed(() => {
    const id = this.openStudentId();
    if (id == null) return null;
    return this.studentReviews()[id] ?? null;
  });

  ngOnInit(): void {
    this.sessionId.set(Number(this.route.snapshot.paramMap.get('sessionId')));
    this.loadClassroom();
  }

  setTab(tab: ClassroomTab): void {
    this.tab.set(tab);
    if (tab === 'reviews') this.ensureReviews(this.reviewKind());
    if (tab === 'exam') this.ensureExamResults();
  }

  setReviewKind(kind: ReviewKind): void {
    this.reviewKind.set(kind);
    this.ensureReviews(kind);
  }

  openStudentReview(studentId: number): void {
    if (this.openStudentId() === studentId) {
      this.openStudentId.set(null);
      return;
    }
    this.openStudentId.set(studentId);
    if (this.studentReviews()[studentId] || this.loadingStudentId() === studentId) return;

    this.loadingStudentId.set(studentId);
    this.sessionsApi
      .getStudentExamReview(this.sessionId(), studentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.studentReviews.update((cache) => ({ ...cache, [studentId]: data }));
          if (this.loadingStudentId() === studentId) this.loadingStudentId.set(null);
        },
        error: (err) => {
          if (this.loadingStudentId() === studentId) this.loadingStudentId.set(null);
          this.error.set(this.apiError(err, 'Failed to load exam review.'));
        },
      });
  }

  downloadMaterial(material: ClassroomMaterialDto): void {
    if (!material.hasFile) return;
    this.downloadingMaterialId.set(material.id);
    this.sessionsApi
      .downloadMaterialFile(this.sessionId(), material.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.downloadingMaterialId.set(null);
          const name = response.fileName || material.originalFileName || material.title;
          const url = URL.createObjectURL(response.data);
          const link = document.createElement('a');
          link.href = url;
          link.download = name;
          link.click();
          URL.revokeObjectURL(url);
        },
        error: (err) => {
          this.downloadingMaterialId.set(null);
          this.error.set(this.apiError(err, 'Failed to download file.'));
        },
      });
  }

  billingLabel(value?: string): string {
    return value === 'Monthly' ? 'lessons.monthly' : 'lessons.perSession';
  }

  price(room: AdminClassroomDto): string | number {
    if (room.billingType === 'Monthly') return room.monthlyPrice ?? '—';
    return room.sessionPrice ?? '—';
  }

  toTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  statusKey(room: AdminClassroomDto): string {
    if (room.hasEnded) return 'adminGroups.ended';
    if (room.hasStarted) return 'adminGroups.started';
    return 'adminGroups.notStarted';
  }

  optionTone(question: { selectedOptionId?: number | null }, option: { id?: number; isCorrect?: boolean }): string {
    if (option.isCorrect) return 'correct';
    if (question.selectedOptionId === option.id) return 'wrong';
    return '';
  }

  private loadClassroom(): void {
    this.loading.set(true);
    this.error.set(null);
    this.sessionsApi.getClassroom(this.sessionId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        this.classroom.set(data);
        this.loading.set(false);
        this.ready.set(true);
      },
      error: (err) => {
        this.loading.set(false);
        this.ready.set(true);
        this.error.set(this.apiError(err, 'Failed to load classroom.'));
      },
    });
  }

  private ensureReviews(kind: ReviewKind): void {
    if (kind === 'session' && this.sessionReviews()) return;
    if (kind === 'lesson' && this.lessonReviews()) return;

    const room = this.classroom();
    if (!room) return;

    this.loadingReviews.set(true);
    const request =
      kind === 'session'
        ? this.sessionsApi.getSessionReviews(this.sessionId())
        : this.overviewApi.getLessonReviews(room.lessonId);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        if (kind === 'session') this.sessionReviews.set(data);
        else this.lessonReviews.set(data);
        this.loadingReviews.set(false);
      },
      error: (err) => {
        this.loadingReviews.set(false);
        this.error.set(this.apiError(err, 'Failed to load reviews.'));
      },
    });
  }

  private ensureExamResults(): void {
    if (this.examResults() || this.loadingExam()) return;
    if (!this.classroom()?.hasExam) return;

    this.loadingExam.set(true);
    this.sessionsApi.getExamResults(this.sessionId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        this.examResults.set(data);
        this.loadingExam.set(false);
      },
      error: (err) => {
        this.loadingExam.set(false);
        this.error.set(this.apiError(err, 'Failed to load exam results.'));
      },
    });
  }

  private apiError(err: unknown, fallback: string): string {
    const e = err as { detail?: string; title?: string; result?: { detail?: string; title?: string } };
    return e?.detail || e?.title || e?.result?.detail || e?.result?.title || fallback;
  }
}
