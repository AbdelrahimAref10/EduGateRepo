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
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';

@Component({
  selector: 'app-teacher-lessons',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, DatePipe, RouterLink, PageLoaderComponent],
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
  readonly educationTypes = signal<EducationTypeDto[]>([]);
  readonly educationStages = signal<EducationStageDto[]>([]);
  readonly educationYears = signal<EducationYearDto[]>([]);
  readonly educationSubjects = signal<EducationSubjectDto[]>([]);
  readonly cityAreas = signal<AreaDto[]>([]);
  readonly selectedTypeId = signal<number | null>(null);
  readonly editingLessonId = signal<number | null>(null);
  readonly formOpen = signal(false);

  readonly filteredLessons = computed(() => {
    const typeId = this.selectedTypeId();
    if (!typeId) return [];
    return this.lessons().filter((lesson) => lesson.educationTypeId === typeId);
  });

  readonly totalLessons = computed(() => this.lessons().length);
  readonly startedLessons = computed(() => this.lessons().filter((lesson) => lesson.hasStarted).length);
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
    startDate: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.loadLessons();
    this.loadEducationTypes();
    this.loadCityAreas();

    this.form.controls.billingType.valueChanges.subscribe(() => {
      this.form.controls.sessionPrice.reset();
      this.form.controls.monthlyPrice.reset();
    });
  }

  loadLessons(): void {
    this.loading.set(true);
    this.error.set(null);

    this.lessonsApi.getMyLessons().subscribe({
      next: (items) => {
        this.lessons.set(items ?? []);
        this.loading.set(false);
        this.ready.set(true);
      },
      error: (err) => {
        this.loading.set(false);
        this.ready.set(true);
        this.error.set(this.apiError(err, 'Failed to load lessons.'));
      },
    });
  }

  selectType(typeId: number): void {
    if (this.selectedTypeId() === typeId && !this.formOpen()) return;

    this.selectedTypeId.set(typeId);
    this.success.set(null);
    this.error.set(null);
    this.closeForm();
    this.resetCurriculum();
    this.loadStages(typeId);
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
    return this.lessons().filter((lesson) => lesson.educationTypeId === typeId).length;
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
      startDate: new Date(value.startDate),
    };

    const editingId = this.editingLessonId();
    this.saving.set(true);

    if (editingId) {
      this.lessonsApi.updateLesson(editingId, new UpdateLessonRequest(payload)).subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.success.set('updated');
          this.lessons.update((items) => items.map((item) => (item.id === updated.id ? updated : item)));
          this.formOpen.set(false);
          this.cancelEdit();
        },
        error: (err) => this.failSave(err, 'Failed to update lesson.'),
      });
      return;
    }

    this.lessonsApi.createLesson(new CreateLessonRequest(payload)).subscribe({
      next: (created) => {
        this.saving.set(false);
        this.success.set('created');
        this.lessons.update((items) => [created, ...items]);
        this.educationYears.set([]);
        this.educationSubjects.set([]);
        this.formOpen.set(false);
        this.cancelEdit();
      },
      error: (err) => this.failSave(err, 'Failed to create lesson.'),
    });
  }

  deleteLesson(lesson: LessonDto): void {
    void this.runDeleteLesson(lesson);
  }

  billingLabel(value?: string): string {
    if (value === 'Monthly') return 'lessons.monthly';
    return 'lessons.perSession';
  }

  private failSave(err: unknown, fallback: string): void {
    this.saving.set(false);
    this.error.set(this.apiError(err, fallback));
  }

  private async runDeleteLesson(lesson: LessonDto): Promise<void> {
    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmDelete',
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.deletingId.set(lesson.id);
    this.error.set(null);
    this.success.set(null);

    this.lessonsApi.deleteLesson(lesson.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('deleted');
        this.lessons.update((items) => items.filter((item) => item.id !== lesson.id));
        if (this.editingLessonId() === lesson.id) this.cancelEdit();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete lesson.'));
      },
    });
  }

  private resetCurriculum(): void {
    this.form.patchValue({
      educationStageId: 0,
      educationYearId: 0,
      educationSubjectId: 0,
    });
    this.educationStages.set([]);
    this.educationYears.set([]);
    this.educationSubjects.set([]);
  }

  private toDateInput(value: Date): string {
    if (!value) return '';
    const iso = value instanceof Date ? value.toISOString() : String(value);
    return iso.slice(0, 10);
  }

  private loadEducationTypes(): void {
    this.loadingTypes.set(true);
    this.educationApi.getTypes(true).subscribe({
      next: (items) => {
        this.educationTypes.set(items ?? []);
        this.loadingTypes.set(false);
      },
      error: (err) => {
        this.loadingTypes.set(false);
        this.error.set(this.apiError(err, 'Failed to load education types.'));
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
      error: (err) => {
        this.loadingStages.set(false);
        this.error.set(this.apiError(err, 'Failed to load education stages.'));
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
      error: (err) => {
        this.loadingYears.set(false);
        this.error.set(this.apiError(err, 'Failed to load education years.'));
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
      error: (err) => {
        this.loadingSubjects.set(false);
        this.error.set(this.apiError(err, 'Failed to load subjects.'));
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
      error: (err) => {
        this.loadingAreas.set(false);
        this.error.set(this.apiError(err, 'Failed to load areas for your city.'));
      },
    });
  }

  private apiError(err: any, fallback: string): string {
    return err?.result?.detail || err?.message || fallback;
  }
}
