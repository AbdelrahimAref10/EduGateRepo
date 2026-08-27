import { Component, OnInit, WritableSignal, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AcademicYearDto,
  AcademicYearsClient,
  CreateAcademicYearRequest,
  CreateEducationStageRequest,
  CreateEducationSubjectRequest,
  CreateEducationYearRequest,
  EducationStageDto,
  EducationStagesClient,
  EducationSubjectDto,
  EducationYearDto,
  UpdateAcademicYearRequest,
  UpdateEducationStageRequest,
  UpdateEducationSubjectRequest,
  UpdateEducationYearRequest,
} from '../../../core/api/academy-api.generated';
import { Observable } from 'rxjs';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-admin-education',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './admin-education.html',
  styleUrl: './admin-education.css',
})
export class AdminEducationComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly academicYearsApi = inject(AcademicYearsClient);
  private readonly stagesApi = inject(EducationStagesClient);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly loading = signal(false);
  readonly loadingStages = signal(false);
  readonly loadingYears = signal(false);
  readonly loadingSubjects = signal(false);
  readonly savingAcademicYear = signal(false);
  readonly savingStage = signal(false);
  readonly savingYear = signal(false);
  readonly savingSubject = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly academicYears = signal<AcademicYearDto[]>([]);
  readonly stages = signal<EducationStageDto[]>([]);
  readonly years = signal<EducationYearDto[]>([]);
  readonly subjects = signal<EducationSubjectDto[]>([]);

  readonly selectedStageId = signal<number | null>(null);
  readonly selectedYearId = signal<number | null>(null);

  readonly editingAcademicYearId = signal<number | null>(null);
  readonly editingStageId = signal<number | null>(null);
  readonly editingYearId = signal<number | null>(null);
  readonly editingSubjectId = signal<number | null>(null);

  readonly academicYearForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    sortOrder: [0, [Validators.required, Validators.min(0)]],
  });
  readonly stageForm = this.nameForm();
  readonly yearForm = this.nameForm();
  readonly subjectForm = this.nameForm();

  ngOnInit(): void {
    this.loadAcademicYears();
    this.loadStages();
  }

  loadAcademicYears(): void {
    this.loading.set(true);
    this.error.set(null);

    this.academicYearsApi.get(false).subscribe({
      next: (items) => {
        this.academicYears.set(this.sortByOrder(items ?? []));
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.apiError(err, 'Failed to load academic years.'));
      },
    });
  }

  loadStages(): void {
    this.loadingStages.set(true);
    this.stagesApi.getStages(false).subscribe({
      next: (items) => {
        this.stages.set(this.sortByOrder(items ?? []));
        this.loadingStages.set(false);
        const selected = this.selectedStageId();
        if (selected && !(items ?? []).some((item) => item.id === selected)) {
          this.clearFromStage();
        }
      },
      error: (err) => {
        this.loadingStages.set(false);
        this.error.set(this.apiError(err, 'Failed to load education stages.'));
      },
    });
  }

  selectStage(stageId: number): void {
    this.selectedStageId.set(stageId);
    this.selectedYearId.set(null);
    this.years.set([]);
    this.subjects.set([]);
    this.cancelEditYear();
    this.cancelEditSubject();
    this.success.set(null);
    this.error.set(null);
    this.loadYears(stageId);
  }

  selectYear(yearId: number): void {
    this.selectedYearId.set(yearId);
    this.subjects.set([]);
    this.cancelEditSubject();
    this.success.set(null);
    this.error.set(null);
    const stageId = this.selectedStageId();
    if (stageId) this.loadSubjects(stageId, yearId);
  }

  saveAcademicYear(): void {
    if (this.editingAcademicYearId()) {
      this.updateAcademicYear();
      return;
    }
    this.createAcademicYear();
  }

  startEditAcademicYear(year: AcademicYearDto): void {
    this.editingAcademicYearId.set(year.id);
    this.academicYearForm.setValue({
      name: year.name,
      sortOrder: year.sortOrder,
    });
    this.clearMessages();
  }

  cancelEditAcademicYear(): void {
    this.editingAcademicYearId.set(null);
    this.academicYearForm.reset({ name: '', sortOrder: 0 });
  }

  saveStage(): void {
    if (this.editingStageId()) {
      this.updateStage();
      return;
    }
    this.createStage();
  }

  startEditStage(stage: EducationStageDto): void {
    this.editingStageId.set(stage.id);
    this.stageForm.setValue({
      nameAr: stage.nameAr,
      nameEn: stage.nameEn,
      sortOrder: stage.sortOrder,
    });
    this.clearMessages();
  }

  cancelEditStage(): void {
    this.editingStageId.set(null);
    this.stageForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
  }

  saveYear(): void {
    if (this.editingYearId()) {
      this.updateYear();
      return;
    }
    this.createYear();
  }

  startEditYear(year: EducationYearDto): void {
    this.editingYearId.set(year.id);
    this.yearForm.setValue({
      nameAr: year.nameAr,
      nameEn: year.nameEn,
      sortOrder: year.sortOrder,
    });
    this.clearMessages();
  }

  cancelEditYear(): void {
    this.editingYearId.set(null);
    this.yearForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
  }

  saveSubject(): void {
    if (this.editingSubjectId()) {
      this.updateSubject();
      return;
    }
    this.createSubject();
  }

  startEditSubject(subject: EducationSubjectDto): void {
    this.editingSubjectId.set(subject.id);
    this.subjectForm.setValue({
      nameAr: subject.nameAr,
      nameEn: subject.nameEn,
      sortOrder: subject.sortOrder,
    });
    this.clearMessages();
  }

  cancelEditSubject(): void {
    this.editingSubjectId.set(null);
    this.subjectForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
  }

  createAcademicYear(): void {
    const value = this.readAcademicYearForm();
    if (!value) return;

    this.savingAcademicYear.set(true);
    this.academicYearsApi
      .create(new CreateAcademicYearRequest({ name: value.name, sortOrder: value.sortOrder }))
      .subscribe({
        next: (created) => {
          this.savingAcademicYear.set(false);
          this.success.set('academicYearCreated');
          this.academicYearForm.reset({ name: '', sortOrder: 0 });
          this.academicYears.update((items) => this.sortByOrder([...items, created]));
        },
        error: (err) => this.failSave(this.savingAcademicYear, err, 'Failed to create academic year.'),
      });
  }

  updateAcademicYear(): void {
    const id = this.editingAcademicYearId();
    const value = this.readAcademicYearForm();
    if (!id || !value) return;

    this.savingAcademicYear.set(true);
    this.academicYearsApi
      .update(id, new UpdateAcademicYearRequest({ name: value.name, sortOrder: value.sortOrder }))
      .subscribe({
        next: (updated) => {
          this.savingAcademicYear.set(false);
          this.success.set('academicYearUpdated');
          this.cancelEditAcademicYear();
          this.academicYears.update((items) =>
            this.sortByOrder(items.map((item) => (item.id === updated.id ? updated : item))),
          );
        },
        error: (err) => this.failSave(this.savingAcademicYear, err, 'Failed to update academic year.'),
      });
  }

  deleteAcademicYear(year: AcademicYearDto): void {
    void this.runDelete({
      messageKey: 'education.confirmDeleteAcademicYear',
      id: `academic-year-${year.id}`,
      successKey: 'academicYearDeleted',
      request: this.academicYearsApi.delete(year.id),
      after: () => {
        this.academicYears.update((items) => items.filter((item) => item.id !== year.id));
        if (this.editingAcademicYearId() === year.id) this.cancelEditAcademicYear();
      },
      fallback: 'Failed to delete academic year.',
    });
  }

  createStage(): void {
    const value = this.readForm(this.stageForm);
    if (!value) return;

    this.savingStage.set(true);
    this.stagesApi
      .createStage(
        new CreateEducationStageRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (created) => {
          this.savingStage.set(false);
          this.success.set('stageCreated');
          this.stageForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
          this.stages.update((items) => this.sortByOrder([...items, created]));
          this.selectStage(created.id);
        },
        error: (err) => this.failSave(this.savingStage, err, 'Failed to create education stage.'),
      });
  }

  updateStage(): void {
    const stageId = this.editingStageId();
    const value = this.readForm(this.stageForm);
    if (!stageId || !value) return;

    this.savingStage.set(true);
    this.stagesApi
      .updateStage(
        stageId,
        new UpdateEducationStageRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (updated) => {
          this.savingStage.set(false);
          this.success.set('stageUpdated');
          this.cancelEditStage();
          this.stages.update((items) =>
            this.sortByOrder(items.map((item) => (item.id === updated.id ? updated : item))),
          );
        },
        error: (err) => this.failSave(this.savingStage, err, 'Failed to update education stage.'),
      });
  }

  deleteStage(stage: EducationStageDto): void {
    void this.runDelete({
      messageKey: 'education.confirmDeleteStage',
      id: `stage-${stage.id}`,
      successKey: 'stageDeleted',
      request: this.stagesApi.deleteStage(stage.id),
      after: () => {
        this.stages.update((items) => items.filter((item) => item.id !== stage.id));
        if (this.selectedStageId() === stage.id) this.clearFromStage();
        if (this.editingStageId() === stage.id) this.cancelEditStage();
      },
      fallback: 'Failed to delete education stage.',
    });
  }

  createYear(): void {
    const stageId = this.selectedStageId();
    const value = this.readForm(this.yearForm);
    if (!stageId) {
      this.error.set('Select an education stage first.');
      return;
    }
    if (!value) return;

    this.savingYear.set(true);
    this.stagesApi
      .createYear(
        stageId,
        new CreateEducationYearRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (created) => {
          this.savingYear.set(false);
          this.success.set('yearCreated');
          this.yearForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
          this.years.update((items) => this.sortByOrder([...items, created]));
          this.bumpStageYears(stageId, 1);
          this.selectYear(created.id);
        },
        error: (err) => this.failSave(this.savingYear, err, 'Failed to create education year.'),
      });
  }

  updateYear(): void {
    const stageId = this.selectedStageId();
    const yearId = this.editingYearId();
    const value = this.readForm(this.yearForm);
    if (!stageId || !yearId || !value) return;

    this.savingYear.set(true);
    this.stagesApi
      .updateYear(
        stageId,
        yearId,
        new UpdateEducationYearRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (updated) => {
          this.savingYear.set(false);
          this.success.set('yearUpdated');
          this.cancelEditYear();
          this.years.update((items) =>
            this.sortByOrder(items.map((item) => (item.id === updated.id ? updated : item))),
          );
        },
        error: (err) => this.failSave(this.savingYear, err, 'Failed to update education year.'),
      });
  }

  deleteYear(year: EducationYearDto): void {
    const stageId = this.selectedStageId();
    if (!stageId) return;

    void this.runDelete({
      messageKey: 'education.confirmDeleteYear',
      id: `year-${year.id}`,
      successKey: 'yearDeleted',
      request: this.stagesApi.deleteYear(stageId, year.id),
      after: () => {
        this.years.update((items) => items.filter((item) => item.id !== year.id));
        this.bumpStageYears(stageId, -1);
        if (this.selectedYearId() === year.id) {
          this.selectedYearId.set(null);
          this.subjects.set([]);
        }
        if (this.editingYearId() === year.id) this.cancelEditYear();
      },
      fallback: 'Failed to delete education year.',
    });
  }

  createSubject(): void {
    const stageId = this.selectedStageId();
    const yearId = this.selectedYearId();
    const value = this.readForm(this.subjectForm);
    if (!stageId || !yearId) {
      this.error.set('Select a grade first.');
      return;
    }
    if (!value) return;

    this.savingSubject.set(true);
    this.stagesApi
      .createSubject(
        stageId,
        yearId,
        new CreateEducationSubjectRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (created) => {
          this.savingSubject.set(false);
          this.success.set('subjectCreated');
          this.subjectForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
          this.subjects.update((items) => this.sortByOrder([...items, created]));
          this.bumpYearSubjects(yearId, 1);
        },
        error: (err) => this.failSave(this.savingSubject, err, 'Failed to create subject.'),
      });
  }

  updateSubject(): void {
    const stageId = this.selectedStageId();
    const yearId = this.selectedYearId();
    const subjectId = this.editingSubjectId();
    const value = this.readForm(this.subjectForm);
    if (!stageId || !yearId || !subjectId || !value) return;

    this.savingSubject.set(true);
    this.stagesApi
      .updateSubject(
        stageId,
        yearId,
        subjectId,
        new UpdateEducationSubjectRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (updated) => {
          this.savingSubject.set(false);
          this.success.set('subjectUpdated');
          this.cancelEditSubject();
          this.subjects.update((items) =>
            this.sortByOrder(items.map((item) => (item.id === updated.id ? updated : item))),
          );
        },
        error: (err) => this.failSave(this.savingSubject, err, 'Failed to update subject.'),
      });
  }

  deleteSubject(subject: EducationSubjectDto): void {
    const stageId = this.selectedStageId();
    const yearId = this.selectedYearId();
    if (!stageId || !yearId) return;

    void this.runDelete({
      messageKey: 'education.confirmDeleteSubject',
      id: `subject-${subject.id}`,
      successKey: 'subjectDeleted',
      request: this.stagesApi.deleteSubject(stageId, yearId, subject.id),
      after: () => {
        this.subjects.update((items) => items.filter((item) => item.id !== subject.id));
        this.bumpYearSubjects(yearId, -1);
        if (this.editingSubjectId() === subject.id) this.cancelEditSubject();
      },
      fallback: 'Failed to delete subject.',
    });
  }

  selectedStageName(): string {
    return this.stages().find((item) => item.id === this.selectedStageId())?.name ?? '';
  }

  selectedYearName(): string {
    return this.years().find((item) => item.id === this.selectedYearId())?.name ?? '';
  }

  private loadYears(stageId: number): void {
    this.loadingYears.set(true);
    this.stagesApi.getYears(stageId, false).subscribe({
      next: (items) => {
        this.years.set(this.sortByOrder(items ?? []));
        this.loadingYears.set(false);
      },
      error: (err) => {
        this.loadingYears.set(false);
        this.error.set(this.apiError(err, 'Failed to load education years.'));
      },
    });
  }

  private loadSubjects(stageId: number, yearId: number): void {
    this.loadingSubjects.set(true);
    this.stagesApi.getSubjects(stageId, yearId, false).subscribe({
      next: (items) => {
        this.subjects.set(this.sortByOrder(items ?? []));
        this.loadingSubjects.set(false);
      },
      error: (err) => {
        this.loadingSubjects.set(false);
        this.error.set(this.apiError(err, 'Failed to load subjects.'));
      },
    });
  }

  private sortByOrder<T extends { sortOrder?: number; nameEn?: string; name?: string }>(items: T[]): T[] {
    return [...items].sort(
      (a, b) =>
        (a.sortOrder ?? 0) - (b.sortOrder ?? 0) ||
        (a.nameEn ?? a.name ?? '').localeCompare(b.nameEn ?? b.name ?? ''),
    );
  }

  private bumpStageYears(stageId: number, delta: number): void {
    this.stages.update((items) =>
      items.map((item) =>
        item.id === stageId
          ? Object.assign(new EducationStageDto(), item, {
              yearsCount: Math.max(0, (item.yearsCount ?? 0) + delta),
            })
          : item,
      ),
    );
  }

  private bumpYearSubjects(yearId: number, delta: number): void {
    this.years.update((items) =>
      items.map((item) =>
        item.id === yearId
          ? Object.assign(new EducationYearDto(), item, {
              subjectsCount: Math.max(0, (item.subjectsCount ?? 0) + delta),
            })
          : item,
      ),
    );
  }

  private clearFromStage(): void {
    this.selectedStageId.set(null);
    this.selectedYearId.set(null);
    this.years.set([]);
    this.subjects.set([]);
  }

  private nameForm() {
    return this.fb.nonNullable.group({
      nameAr: ['', [Validators.required, Validators.maxLength(150)]],
      nameEn: ['', [Validators.required, Validators.maxLength(150)]],
      sortOrder: [0, [Validators.required, Validators.min(0)]],
    });
  }

  private readAcademicYearForm() {
    this.clearMessages();
    if (this.academicYearForm.invalid) {
      this.academicYearForm.markAllAsTouched();
      return null;
    }
    const value = this.academicYearForm.getRawValue();
    return {
      name: value.name.trim(),
      sortOrder: value.sortOrder,
    };
  }

  private readForm(form: ReturnType<AdminEducationComponent['nameForm']>) {
    this.clearMessages();
    if (form.invalid) {
      form.markAllAsTouched();
      return null;
    }
    const value = form.getRawValue();
    return {
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      sortOrder: value.sortOrder,
    };
  }

  private clearMessages(): void {
    this.error.set(null);
    this.success.set(null);
  }

  private failSave(saving: WritableSignal<boolean>, err: unknown, fallback: string): void {
    saving.set(false);
    this.error.set(this.apiError(err, fallback));
  }

  private async runDelete(options: {
    messageKey: string;
    id: string;
    successKey: string;
    request: Observable<void>;
    after: () => void;
    fallback: string;
  }): Promise<void> {
    const ok = await this.confirmDialog.ask({
      messageKey: options.messageKey,
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.clearMessages();
    this.deletingId.set(options.id);

    options.request.subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set(options.successKey);
        options.after();
      },
      error: (err: unknown) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, options.fallback));
      },
    });
  }

  private apiError(err: any, fallback: string): string {
    return err?.result?.detail || err?.message || fallback;
  }
}
