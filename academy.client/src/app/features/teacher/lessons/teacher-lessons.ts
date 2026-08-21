import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  AreaDto,
  BillingType,
  CreateLessonRequest,
  EducationTypeDto,
  EducationTypesClient,
  EducationYearDto,
  LessonDto,
  LessonsClient,
  UpdateLessonRequest,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-teacher-lessons',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, DatePipe, RouterLink],
  templateUrl: './teacher-lessons.html',
  styleUrl: './teacher-lessons.css',
})
export class TeacherLessonsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly educationApi = inject(EducationTypesClient);
  private readonly i18n = inject(TranslationService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly BillingType = BillingType;
  readonly loading = signal(false);
  readonly loadingTypes = signal(false);
  readonly loadingYears = signal(false);
  readonly loadingAreas = signal(false);
  readonly saving = signal(false);
  readonly deletingId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly lessons = signal<LessonDto[]>([]);
  readonly educationTypes = signal<EducationTypeDto[]>([]);
  readonly educationYears = signal<EducationYearDto[]>([]);
  readonly cityAreas = signal<AreaDto[]>([]);
  readonly selectedTypeId = signal<number | null>(null);
  readonly editingLessonId = signal<number | null>(null);

  readonly filteredLessons = computed(() => {
    const typeId = this.selectedTypeId();
    if (!typeId) return [];
    return this.lessons().filter((lesson) => lesson.educationTypeId === typeId);
  });

  readonly form = this.fb.nonNullable.group({
    subject: ['', [Validators.required, Validators.maxLength(200)]],
    educationYearId: [0, [Validators.required, Validators.min(1)]],
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
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load lessons.');
      },
    });
  }

  selectType(typeId: number): void {
    this.selectedTypeId.set(typeId);
    this.success.set(null);
    this.error.set(null);
    this.cancelEdit();
    this.form.controls.educationYearId.setValue(0);
    this.educationYears.set([]);
    this.loadEducationYears(typeId);
  }

  selectedTypeName(): string {
    const type = this.educationTypes().find((item) => item.id === this.selectedTypeId());
    return type ? type.name : '';
  }

  lessonsCountForType(typeId: number): number {
    return this.lessons().filter((lesson) => lesson.educationTypeId === typeId).length;
  }

  startEdit(lesson: LessonDto): void {
    if (!lesson.canEdit) return;

    this.error.set(null);
    this.success.set(null);
    this.editingLessonId.set(lesson.id);
    this.selectedTypeId.set(lesson.educationTypeId);
    this.loadEducationYears(lesson.educationTypeId);

    const billingType =
      lesson.billingType === 'Monthly' ? BillingType.Monthly : BillingType.PerSession;

    this.form.setValue({
      subject: lesson.subject,
      educationYearId: lesson.educationYearId,
      areaId: lesson.areaId,
      billingType,
      sessionPrice: lesson.sessionPrice ?? null,
      monthlyPrice: lesson.monthlyPrice ?? null,
      startDate: this.toDateInput(lesson.startDate),
    });
  }

  cancelEdit(): void {
    this.editingLessonId.set(null);
    const keptAreaId = this.form.controls.areaId.value || 0;
    this.form.reset({
      subject: '',
      educationYearId: 0,
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

    const editingId = this.editingLessonId();
    this.saving.set(true);

    if (editingId) {
      const request = new UpdateLessonRequest({
        subject: value.subject.trim(),
        educationTypeId: typeId,
        educationYearId: value.educationYearId,
        areaId: value.areaId,
        billingType,
        sessionPrice: billingType === BillingType.PerSession ? value.sessionPrice! : undefined,
        monthlyPrice: billingType === BillingType.Monthly ? value.monthlyPrice! : undefined,
        startDate: new Date(value.startDate),
      });

      this.lessonsApi.updateLesson(editingId, request).subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.success.set('updated');
          this.lessons.update((items) => items.map((item) => (item.id === updated.id ? updated : item)));
          this.cancelEdit();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to update lesson.');
        },
      });
      return;
    }

    const request = new CreateLessonRequest({
      subject: value.subject.trim(),
      educationTypeId: typeId,
      educationYearId: value.educationYearId,
      areaId: value.areaId,
      billingType,
      sessionPrice: billingType === BillingType.PerSession ? value.sessionPrice! : undefined,
      monthlyPrice: billingType === BillingType.Monthly ? value.monthlyPrice! : undefined,
      startDate: new Date(value.startDate),
    });

    this.lessonsApi.createLesson(request).subscribe({
      next: (created) => {
        this.saving.set(false);
        this.success.set('created');
        const keptAreaId = value.areaId;
        this.form.reset({
          subject: '',
          educationYearId: 0,
          areaId: keptAreaId,
          billingType: BillingType.PerSession,
          sessionPrice: null,
          monthlyPrice: null,
          startDate: '',
        });
        this.lessons.update((items) => [created, ...items]);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to create lesson.');
      },
    });
  }

  deleteLesson(lesson: LessonDto): void {
    void this.runDeleteLesson(lesson);
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
        this.error.set(err?.result?.detail || err?.message || 'Failed to delete lesson.');
      },
    });
  }

  billingLabel(value?: string): string {
    if (value === 'Monthly') return 'lessons.monthly';
    return 'lessons.perSession';
  }

  label(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
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
        this.error.set(err?.result?.detail || err?.message || 'Failed to load education types.');
      },
    });
  }

  private loadEducationYears(typeId: number): void {
    this.loadingYears.set(true);
    this.educationApi.getYears(typeId, true).subscribe({
      next: (items) => {
        this.educationYears.set(items ?? []);
        this.loadingYears.set(false);
      },
      error: (err) => {
        this.loadingYears.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load education years.');
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
        this.error.set(err?.result?.detail || err?.message || 'Failed to load areas for your city.');
      },
    });
  }
}
