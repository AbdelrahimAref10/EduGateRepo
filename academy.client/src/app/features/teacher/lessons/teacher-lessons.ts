import { DatePipe } from '@angular/common';
import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  AreaDto,
  BillingType,
  CreateLessonRequest,
  EducationStageDto,
  EducationSubjectDto,
  EducationTypeDto,
  EducationTypesClient,
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
  private readonly educationApi = inject(EducationTypesClient);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly BillingType = BillingType;
  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly loadingTypes = signal(false);
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
  readonly typeCounts = signal<Record<number, number>>({});
  readonly educationTypes = signal<EducationTypeDto[]>([]);
  readonly educationStages = signal<EducationStageDto[]>([]);
  readonly educationYears = signal<EducationYearDto[]>([]);
  readonly educationSubjects = signal<EducationSubjectDto[]>([]);
  readonly cityAreas = signal<AreaDto[]>([]);
  readonly selectedTypeId = signal<number | null>(null);
  readonly editingLessonId = signal<number | null>(null);
  readonly formOpen = signal(false);

  readonly selectedType = computed(
    () => this.educationTypes().find((item) => item.id === this.selectedTypeId()) ?? null,
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

  ngOnInit(): void {
    this.loadEducationTypes();
    this.loadCityAreas();
    this.loadAllLessonsCount();
    this.ready.set(true);
    this.loading.set(false);

    this.form.controls.billingType.valueChanges.subscribe(() => {
      this.form.controls.sessionPrice.reset();
      this.form.controls.monthlyPrice.reset();
      this.form.controls.chargeAbsentSessions.setValue(false);
    });
  }

  refresh(): void {
    this.loadAllLessonsCount();
    this.loadTypeCounts(this.educationTypes());
    if (this.selectedTypeId()) this.loadLessons();
  }

  loadAllLessonsCount(): void {
    this.lessonsApi.getMyLessons(undefined, 1, 1).subscribe({
      next: (data) => this.allLessonsCount.set(data.totalCount),
      error: () => undefined,
    });
  }

  loadTypeCounts(types: EducationTypeDto[]): void {
    if (!types.length) {
      this.typeCounts.set({});
      return;
    }

    for (const type of types) {
      this.lessonsApi.getMyLessons(type.id, 1, 1).subscribe({
        next: (data) => {
          this.typeCounts.update((map) => ({ ...map, [type.id]: data.totalCount }));
        },
        error: () => {
          this.typeCounts.update((map) => ({ ...map, [type.id]: 0 }));
        },
      });
    }
  }

  loadLessons(showSpinner = true): void {
    const typeId = this.selectedTypeId();
    if (!typeId) {
      this.lessons.set([]);
      this.totalCount.set(0);
      return;
    }

    if (showSpinner) this.loading.set(true);
    this.error.set(null);

    this.lessonsApi.getMyLessons(typeId, this.page(), this.pageSize).subscribe({
      next: (data) => {
        this.lessons.set(data.items ?? []);
        this.totalCount.set(data.totalCount);
        this.page.set(data.page);
        this.typeCounts.update((map) => ({ ...map, [typeId]: data.totalCount }));
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.apiError(err, 'Failed to load lessons.'));
      },
    });
  }

  onPageChange(page: number): void {
    if (page === this.page()) return;
    this.page.set(page);
    this.loadLessons();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  selectType(typeId: number): void {
    if (this.selectedTypeId() === typeId && !this.formOpen()) return;

    this.selectedTypeId.set(typeId);
    this.page.set(1);
    this.success.set(null);
    this.error.set(null);
    this.closeForm();
    this.resetCurriculum();
    this.loadStages(typeId);
    this.loadLessons();
  }

  openCreate(): void {
    if (!this.selectedTypeId()) return;
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

  lessonsCountForType(typeId: number): number {
    return this.typeCounts()[typeId] ?? 0;
  }

  onStagePicked(): void {
    const typeId = this.selectedTypeId();
    const stageId = this.form.controls.educationStageId.value;
    this.form.patchValue({ educationYearId: 0, educationSubjectId: 0 });
    this.educationYears.set([]);
    this.educationSubjects.set([]);
    if (typeId && stageId) this.loadYears(typeId, stageId);
  }

  onYearPicked(): void {
    const typeId = this.selectedTypeId();
    const stageId = this.form.controls.educationStageId.value;
    const yearId = this.form.controls.educationYearId.value;
    this.form.patchValue({ educationSubjectId: 0 });
    this.educationSubjects.set([]);
    if (typeId && stageId && yearId) this.loadSubjects(typeId, stageId, yearId);
  }

  startEdit(lesson: LessonDto): void {
    if (!lesson.canEdit) return;

    this.error.set(null);
    this.success.set(null);
    this.editingLessonId.set(lesson.id);
    this.selectedTypeId.set(lesson.educationTypeId);
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

    this.loadStages(lesson.educationTypeId);
    this.loadYears(lesson.educationTypeId, lesson.educationStageId);
    this.loadSubjects(lesson.educationTypeId, lesson.educationStageId, lesson.educationYearId);
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

    const typeId = this.selectedTypeId();
    if (!typeId) {
      this.error.set('Select an education type first.');
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
      educationTypeId: typeId,
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
          this.loadAllLessonsCount();
          this.loadTypeCounts(this.educationTypes());
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
        this.loadAllLessonsCount();
        this.loadTypeCounts(this.educationTypes());
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
        this.loadAllLessonsCount();
        this.loadTypeCounts(this.educationTypes());
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

  private loadEducationTypes(): void {
    this.loadingTypes.set(true);
    this.educationApi.getTypes(true).subscribe({
      next: (items) => {
        const types = items ?? [];
        this.educationTypes.set(types);
        this.loadingTypes.set(false);
        this.loadTypeCounts(types);
      },
      error: () => {
        this.loadingTypes.set(false);
        this.error.set('Failed to load education types.');
      },
    });
  }

  private loadStages(typeId: number): void {
    this.loadingStages.set(true);
    this.educationApi.getStages(typeId, true).subscribe({
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

  private loadYears(typeId: number, stageId: number): void {
    this.loadingYears.set(true);
    this.educationApi.getYears(typeId, stageId, true).subscribe({
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

  private loadSubjects(typeId: number, stageId: number, yearId: number): void {
    this.loadingSubjects.set(true);
    this.educationApi.getSubjects(typeId, stageId, yearId, true).subscribe({
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
    this.educationStages.set([]);
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
