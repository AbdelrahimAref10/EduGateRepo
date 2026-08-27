import { DatePipe } from '@angular/common';
import { Component, DestroyRef, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, catchError, of, switchMap } from 'rxjs';
import {
  AcademicYearDto,
  AcademicYearsClient,
  AreaDto,
  BillingType,
  CreateLessonRequest,
  EducationStageDto,
  EducationStagesClient,
  EducationSubjectDto,
  EducationYearDto,
  LessonDto,
  LessonsClient,
  UpdateLessonRequest,
} from '../../../core/api/academy-api.generated';
import { DEFAULT_PAGE_SIZE } from '../../../core/api/paging';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { PaginatorComponent } from '../../../shared/paginator/paginator';

@Component({
  selector: 'app-teacher-lessons',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    DatePipe,
    RouterLink,
    PageLoaderComponent,
    PaginatorComponent,
  ],
  templateUrl: './teacher-lessons.html',
  styleUrl: './teacher-lessons.css',
})
export class TeacherLessonsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly academicYearsApi = inject(AcademicYearsClient);
  private readonly stagesApi = inject(EducationStagesClient);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly list$ = new Subject<boolean>();
  private readonly counts$ = new Subject<void>();

  readonly BillingType = BillingType;
  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly loadingAcademicYears = signal(false);
  readonly loadingStages = signal(false);
  readonly loadingYears = signal(false);
  readonly loadingSubjects = signal(false);
  readonly loadingAreas = signal(false);
  readonly saving = signal(false);
  readonly deletingId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly lessons = signal<LessonDto[]>([]);
  readonly page = signal(1);
  readonly pageSize = DEFAULT_PAGE_SIZE;
  readonly totalCount = signal(0);
  readonly allLessonsCount = signal(0);
  readonly yearCounts = signal<Record<number, number>>({});
  readonly academicYears = signal<AcademicYearDto[]>([]);
  readonly educationStages = signal<EducationStageDto[]>([]);
  readonly educationYears = signal<EducationYearDto[]>([]);
  readonly educationSubjects = signal<EducationSubjectDto[]>([]);
  readonly cityAreas = signal<AreaDto[]>([]);
  readonly selectedAcademicYearId = signal<number | null>(null);
  readonly editingLessonId = signal<number | null>(null);
  readonly formOpen = signal(false);

  readonly selectedAcademicYear = computed(
    () => this.academicYears().find((item) => item.id === this.selectedAcademicYearId()) ?? null,
  );

  readonly form = this.fb.nonNullable.group({
    educationStageId: [0, [Validators.required, Validators.min(1)]],
    educationYearId: [0, [Validators.required, Validators.min(1)]],
    educationSubjectId: [0, [Validators.required, Validators.min(1)]],
    areaId: [0, [Validators.required, Validators.min(1)]],
    billingType: [BillingType.PerSession as BillingType, [Validators.required]],
    sessionPrice: [null as number | null],
    monthlyPrice: [null as number | null],
    chargeAbsentSessions: [false],
    startDate: ['', [Validators.required]],
  });

  constructor() {
    this.counts$
      .pipe(
        switchMap(() =>
          this.lessonsApi.getMyLessonCounts().pipe(
            catchError(() => of(null)),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        this.allLessonsCount.set(data?.total ?? 0);
        const map: Record<number, number> = {};
        for (const row of data?.byAcademicYear ?? []) {
          map[row.academicYearId] = row.count;
        }
        this.yearCounts.set(map);
      });

    this.list$
      .pipe(
        switchMap((showSpinner) => {
          const yearId = this.selectedAcademicYearId();
          if (!yearId) {
            this.lessons.set([]);
            this.totalCount.set(0);
            this.loading.set(false);
            return of(null);
          }

          if (showSpinner) this.loading.set(true);
          this.error.set(null);
          return this.lessonsApi.getMyLessons(yearId, undefined, this.page(), this.pageSize).pipe(
            catchError((err) => {
              this.loading.set(false);
              this.error.set(this.apiError(err, 'Failed to load lessons.'));
              return of(null);
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        if (!data) return;
        const yearId = this.selectedAcademicYearId();
        this.lessons.set(data.items ?? []);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        if (yearId) {
          this.yearCounts.update((map) => ({ ...map, [yearId]: data.totalCount }));
        }
        this.loading.set(false);
      });
  }

  ngOnInit(): void {
    this.loadAcademicYears();
    this.loadEducationStages();
    this.loadCityAreas();
    this.counts$.next();
    this.ready.set(true);
    this.loading.set(false);

    this.form.controls.billingType.valueChanges.subscribe(() => {
      this.form.controls.sessionPrice.reset();
      this.form.controls.monthlyPrice.reset();
      this.form.controls.chargeAbsentSessions.setValue(false);
    });
  }

  refresh(): void {
    this.counts$.next();
    if (this.selectedAcademicYearId()) this.loadLessons();
  }

  loadLessons(showSpinner = true): void {
    this.list$.next(showSpinner);
  }

  onPageChange(page: number): void {
    if (page === this.page()) return;
    this.page.set(page);
    this.loadLessons();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  selectAcademicYear(yearId: number): void {
    if (this.selectedAcademicYearId() === yearId && !this.formOpen()) return;

    this.selectedAcademicYearId.set(yearId);
    this.page.set(1);
    this.success.set(null);
    this.error.set(null);
    this.closeForm();
    this.resetCurriculum();
    this.loadLessons();
  }

  openCreate(): void {
    if (!this.selectedAcademicYearId()) return;
    this.error.set(null);
    this.success.set(null);
    this.cancelEdit();
    this.formOpen.set(true);
  }

  closeForm(): void {
    if (this.saving()) return;
    this.formOpen.set(false);
    this.cancelEdit();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.formOpen()) this.closeForm();
  }

  lessonsCountForYear(yearId: number): number {
    return this.yearCounts()[yearId] ?? 0;
  }

  onStagePicked(): void {
    const stageId = this.form.controls.educationStageId.value;
    this.form.patchValue({ educationYearId: 0, educationSubjectId: 0 });
    this.educationYears.set([]);
    this.educationSubjects.set([]);
    if (stageId) this.loadYears(stageId);
  }

  onYearPicked(): void {
    const stageId = this.form.controls.educationStageId.value;
    const yearId = this.form.controls.educationYearId.value;
    this.form.patchValue({ educationSubjectId: 0 });
    this.educationSubjects.set([]);
    if (stageId && yearId) this.loadSubjects(stageId, yearId);
  }

  startEdit(lesson: LessonDto): void {
    if (!lesson.canEdit) return;

    this.error.set(null);
    this.success.set(null);
    this.editingLessonId.set(lesson.id);
    this.selectedAcademicYearId.set(lesson.academicYearId);
    this.formOpen.set(true);

    const billingType =
      lesson.billingType === 'Monthly' ? BillingType.Monthly : BillingType.PerSession;

    this.form.setValue({
      educationStageId: lesson.educationStageId,
      educationYearId: lesson.educationYearId,
      educationSubjectId: lesson.educationSubjectId,
      areaId: lesson.areaId,
      billingType,
      sessionPrice: lesson.sessionPrice ?? null,
      monthlyPrice: lesson.monthlyPrice ?? null,
      chargeAbsentSessions: !!lesson.chargeAbsentSessions,
      startDate: this.toDateInput(lesson.startDate),
    });

    this.loadYears(lesson.educationStageId);
    this.loadSubjects(lesson.educationStageId, lesson.educationYearId);
  }

  cancelEdit(): void {
    this.editingLessonId.set(null);
    const keptAreaId = this.form.controls.areaId.value || 0;
    this.form.reset({
      educationStageId: 0,
      educationYearId: 0,
      educationSubjectId: 0,
      areaId: keptAreaId,
      billingType: BillingType.PerSession,
      sessionPrice: null,
      monthlyPrice: null,
      chargeAbsentSessions: false,
      startDate: '',
    });
  }

  submit(): void {
    this.error.set(null);
    this.success.set(null);

    const yearId = this.selectedAcademicYearId();
    if (!yearId) {
      this.error.set('Select an academic year first.');
      return;
    }

    const value = this.form.getRawValue();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const billingType = Number(value.billingType) as BillingType;

    if (billingType === BillingType.PerSession && !(value.sessionPrice && value.sessionPrice > 0)) {
      this.error.set('Session price is required.');
      return;
    }

    if (billingType === BillingType.Monthly && !(value.monthlyPrice && value.monthlyPrice > 0)) {
      this.error.set('Monthly price is required.');
      return;
    }

    const payload = {
      academicYearId: yearId,
      educationStageId: value.educationStageId,
      educationYearId: value.educationYearId,
      educationSubjectId: value.educationSubjectId,
      areaId: value.areaId,
      billingType,
      sessionPrice: billingType === BillingType.PerSession ? value.sessionPrice! : undefined,
      monthlyPrice: billingType === BillingType.Monthly ? value.monthlyPrice! : undefined,
      chargeAbsentSessions:
        billingType === BillingType.PerSession ? !!value.chargeAbsentSessions : false,
      startDate: new Date(value.startDate),
    };

    const editingId = this.editingLessonId();
    this.saving.set(true);

    if (editingId) {
      this.lessonsApi.updateLesson(editingId, new UpdateLessonRequest(payload)).subscribe({
        next: () => {
          this.saving.set(false);
          this.success.set('updated');
          this.closeForm();
          this.loadLessons(false);
          this.counts$.next();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(this.apiError(err, 'Failed to update lesson.'));
        },
      });
      return;
    }

    this.lessonsApi.createLesson(new CreateLessonRequest(payload)).subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set('created');
        this.closeForm();
        this.page.set(1);
        this.loadLessons(false);
        this.counts$.next();
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(this.apiError(err, 'Failed to create lesson.'));
      },
    });
  }

  deleteLesson(lesson: LessonDto): void {
    void this.runDeleteLesson(lesson);
  }

  private async runDeleteLesson(lesson: LessonDto): Promise<void> {
    const ok = await this.confirmDialog.ask({
      titleKey: 'lessons.delete',
      messageKey: 'lessons.deleteConfirm',
      confirmKey: 'lessons.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.deletingId.set(lesson.id);
    this.error.set(null);
    this.lessonsApi.deleteLesson(lesson.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('deleted');
        this.loadLessons(false);
        this.counts$.next();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete lesson.'));
      },
    });
  }

  billingLabel(value?: string): string {
    return value === 'Monthly' ? 'lessons.monthly' : 'lessons.perSession';
  }

  toDateInput(value?: Date | string): string {
    if (!value) return '';
    const d = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(d.getTime())) return '';
    return d.toISOString().slice(0, 10);
  }

  private loadAcademicYears(): void {
    this.loadingAcademicYears.set(true);
    this.academicYearsApi.get(true).subscribe({
      next: (items) => {
        this.academicYears.set(items ?? []);
        this.loadingAcademicYears.set(false);
        this.counts$.next();
      },
      error: () => {
        this.loadingAcademicYears.set(false);
        this.error.set('Failed to load academic years.');
      },
    });
  }

  private loadEducationStages(): void {
    this.loadingStages.set(true);
    this.stagesApi.getStages(true).subscribe({
      next: (items) => {
        this.educationStages.set(items ?? []);
        this.loadingStages.set(false);
      },
      error: () => {
        this.loadingStages.set(false);
        this.educationStages.set([]);
      },
    });
  }

  private loadYears(stageId: number): void {
    this.loadingYears.set(true);
    this.stagesApi.getYears(stageId, true).subscribe({
      next: (items) => {
        this.educationYears.set(items ?? []);
        this.loadingYears.set(false);
      },
      error: () => {
        this.loadingYears.set(false);
        this.educationYears.set([]);
      },
    });
  }

  private loadSubjects(stageId: number, yearId: number): void {
    this.loadingSubjects.set(true);
    this.stagesApi.getSubjects(stageId, yearId, true).subscribe({
      next: (items) => {
        this.educationSubjects.set(items ?? []);
        this.loadingSubjects.set(false);
      },
      error: () => {
        this.loadingSubjects.set(false);
        this.educationSubjects.set([]);
      },
    });
  }

  private loadCityAreas(): void {
    this.loadingAreas.set(true);
    this.lessonsApi.getMyCityAreas().subscribe({
      next: (items) => {
        this.cityAreas.set(items ?? []);
        this.loadingAreas.set(false);
      },
      error: () => {
        this.loadingAreas.set(false);
        this.cityAreas.set([]);
      },
    });
  }

  private resetCurriculum(): void {
    this.educationYears.set([]);
    this.educationSubjects.set([]);
    this.form.patchValue({
      educationStageId: 0,
      educationYearId: 0,
      educationSubjectId: 0,
    });
  }

  private apiError(err: unknown, fallback: string): string {
    const e = err as {
      detail?: string;
      title?: string;
      error?: { detail?: string };
      result?: { detail?: string; title?: string };
    };
    return e?.detail || e?.title || e?.error?.detail || e?.result?.detail || e?.result?.title || fallback;
  }
}
