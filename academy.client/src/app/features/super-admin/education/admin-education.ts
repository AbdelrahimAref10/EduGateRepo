import { Component, OnInit, WritableSignal, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CreateEducationStageRequest,
  CreateEducationSubjectRequest,
  CreateEducationTypeRequest,
  CreateEducationYearRequest,
  EducationStageDto,
  EducationSubjectDto,
  EducationTypeDto,
  EducationTypesClient,
  EducationYearDto,
  UpdateEducationStageRequest,
  UpdateEducationSubjectRequest,
  UpdateEducationTypeRequest,
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
  private readonly api = inject(EducationTypesClient);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly loading = signal(false);
  readonly loadingStages = signal(false);
  readonly loadingYears = signal(false);
  readonly loadingSubjects = signal(false);
  readonly savingType = signal(false);
  readonly savingStage = signal(false);
  readonly savingYear = signal(false);
  readonly savingSubject = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly types = signal<EducationTypeDto[]>([]);
  readonly stages = signal<EducationStageDto[]>([]);
  readonly years = signal<EducationYearDto[]>([]);
  readonly subjects = signal<EducationSubjectDto[]>([]);

  readonly selectedTypeId = signal<number | null>(null);
  readonly selectedStageId = signal<number | null>(null);
  readonly selectedYearId = signal<number | null>(null);

  readonly editingTypeId = signal<number | null>(null);
  readonly editingStageId = signal<number | null>(null);
  readonly editingYearId = signal<number | null>(null);
  readonly editingSubjectId = signal<number | null>(null);

  readonly typeForm = this.nameForm();
  readonly stageForm = this.nameForm();
  readonly yearForm = this.nameForm();
  readonly subjectForm = this.nameForm();

  ngOnInit(): void {
    this.loadTypes();
  }

  loadTypes(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getTypes(false).subscribe({
      next: (items) => {
        this.types.set(items ?? []);
        this.loading.set(false);

        const selected = this.selectedTypeId();
        if (selected && !(items ?? []).some((item) => item.id === selected)) {
          this.clearFromType();
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.apiError(err, 'Failed to load education types.'));
      },
    });
  }

  selectType(typeId: number): void {
    this.selectedTypeId.set(typeId);
    this.selectedStageId.set(null);
    this.selectedYearId.set(null);
    this.stages.set([]);
    this.years.set([]);
    this.subjects.set([]);
    this.cancelEditStage();
    this.cancelEditYear();
    this.cancelEditSubject();
    this.success.set(null);
    this.error.set(null);
    this.loadStages(typeId);
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
    const typeId = this.selectedTypeId();
    if (typeId) this.loadYears(typeId, stageId);
  }

  selectYear(yearId: number): void {
    this.selectedYearId.set(yearId);
    this.subjects.set([]);
    this.cancelEditSubject();
    this.success.set(null);
    this.error.set(null);
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    if (typeId && stageId) this.loadSubjects(typeId, stageId, yearId);
  }

  saveType(): void {
    if (this.editingTypeId()) {
      this.updateType();
      return;
    }
    this.createType();
  }

  startEditType(type: EducationTypeDto): void {
    this.editingTypeId.set(type.id);
    this.typeForm.setValue({
      nameAr: type.nameAr,
      nameEn: type.nameEn,
      sortOrder: type.sortOrder,
    });
    this.clearMessages();
  }

  cancelEditType(): void {
    this.editingTypeId.set(null);
    this.typeForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
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

  createType(): void {
    const value = this.readForm(this.typeForm);
    if (!value) return;

    this.savingType.set(true);
    this.api
      .createType(
        new CreateEducationTypeRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (created) => {
          this.savingType.set(false);
          this.success.set('typeCreated');
          this.typeForm.reset({ nameAr: '', nameEn: '', sortOrder: 0 });
          this.types.update((items) => this.sortByOrder([...items, created]));
          this.selectType(created.id);
        },
        error: (err) => this.failSave(this.savingType, err, 'Failed to create education type.'),
      });
  }

  updateType(): void {
    const typeId = this.editingTypeId();
    const value = this.readForm(this.typeForm);
    if (!typeId || !value) return;

    this.savingType.set(true);
    this.api
      .updateType(
        typeId,
        new UpdateEducationTypeRequest({
          nameAr: value.nameAr,
          nameEn: value.nameEn,
          sortOrder: value.sortOrder,
        }),
      )
      .subscribe({
        next: (updated) => {
          this.savingType.set(false);
          this.success.set('typeUpdated');
          this.cancelEditType();
          this.types.update((items) =>
            this.sortByOrder(items.map((item) => (item.id === updated.id ? updated : item))),
          );
        },
        error: (err) => this.failSave(this.savingType, err, 'Failed to update education type.'),
      });
  }

  deleteType(type: EducationTypeDto): void {
    void this.runDelete({
      messageKey: 'education.confirmDeleteType',
      id: `type-${type.id}`,
      successKey: 'typeDeleted',
      request: this.api.deleteType(type.id),
      after: () => {
        this.types.update((items) => items.filter((item) => item.id !== type.id));
        if (this.selectedTypeId() === type.id) this.clearFromType();
        if (this.editingTypeId() === type.id) this.cancelEditType();
      },
      fallback: 'Failed to delete education type.',
    });
  }

  createStage(): void {
    const typeId = this.selectedTypeId();
    const value = this.readForm(this.stageForm);
    if (!typeId) {
      this.error.set('Select an education type first.');
      return;
    }
    if (!value) return;

    this.savingStage.set(true);
    this.api
      .createStage(
        typeId,
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
          this.bumpTypeStages(typeId, 1);
          this.selectStage(created.id);
        },
        error: (err) => this.failSave(this.savingStage, err, 'Failed to create education stage.'),
      });
  }

  updateStage(): void {
    const typeId = this.selectedTypeId();
    const stageId = this.editingStageId();
    const value = this.readForm(this.stageForm);
    if (!typeId || !stageId || !value) return;

    this.savingStage.set(true);
    this.api
      .updateStage(
        typeId,
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
    const typeId = this.selectedTypeId();
    if (!typeId) return;

    void this.runDelete({
      messageKey: 'education.confirmDeleteStage',
      id: `stage-${stage.id}`,
      successKey: 'stageDeleted',
      request: this.api.deleteStage(typeId, stage.id),
      after: () => {
        this.stages.update((items) => items.filter((item) => item.id !== stage.id));
        this.bumpTypeStages(typeId, -1);
        if (this.selectedStageId() === stage.id) {
          this.selectedStageId.set(null);
          this.selectedYearId.set(null);
          this.years.set([]);
          this.subjects.set([]);
        }
        if (this.editingStageId() === stage.id) this.cancelEditStage();
      },
      fallback: 'Failed to delete education stage.',
    });
  }

  createYear(): void {
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    const value = this.readForm(this.yearForm);
    if (!typeId || !stageId) {
      this.error.set('Select an education stage first.');
      return;
    }
    if (!value) return;

    this.savingYear.set(true);
    this.api
      .createYear(
        typeId,
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
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    const yearId = this.editingYearId();
    const value = this.readForm(this.yearForm);
    if (!typeId || !stageId || !yearId || !value) return;

    this.savingYear.set(true);
    this.api
      .updateYear(
        typeId,
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
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    if (!typeId || !stageId) return;

    void this.runDelete({
      messageKey: 'education.confirmDeleteYear',
      id: `year-${year.id}`,
      successKey: 'yearDeleted',
      request: this.api.deleteYear(typeId, stageId, year.id),
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
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    const yearId = this.selectedYearId();
    const value = this.readForm(this.subjectForm);
    if (!typeId || !stageId || !yearId) {
      this.error.set('Select a study year first.');
      return;
    }
    if (!value) return;

    this.savingSubject.set(true);
    this.api
      .createSubject(
        typeId,
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
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    const yearId = this.selectedYearId();
    const subjectId = this.editingSubjectId();
    const value = this.readForm(this.subjectForm);
    if (!typeId || !stageId || !yearId || !subjectId || !value) return;

    this.savingSubject.set(true);
    this.api
      .updateSubject(
        typeId,
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
    const typeId = this.selectedTypeId();
    const stageId = this.selectedStageId();
    const yearId = this.selectedYearId();
    if (!typeId || !stageId || !yearId) return;

    void this.runDelete({
      messageKey: 'education.confirmDeleteSubject',
      id: `subject-${subject.id}`,
      successKey: 'subjectDeleted',
      request: this.api.deleteSubject(typeId, stageId, yearId, subject.id),
      after: () => {
        this.subjects.update((items) => items.filter((item) => item.id !== subject.id));
        this.bumpYearSubjects(yearId, -1);
        if (this.editingSubjectId() === subject.id) this.cancelEditSubject();
      },
      fallback: 'Failed to delete subject.',
    });
  }

  selectedTypeName(): string {
    return this.types().find((item) => item.id === this.selectedTypeId())?.name ?? '';
  }

  selectedStageName(): string {
    return this.stages().find((item) => item.id === this.selectedStageId())?.name ?? '';
  }

  selectedYearName(): string {
    return this.years().find((item) => item.id === this.selectedYearId())?.name ?? '';
  }

  private loadStages(typeId: number): void {
    this.loadingStages.set(true);
    this.api.getStages(typeId, false).subscribe({
      next: (items) => {
        this.stages.set(items ?? []);
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
    this.api.getYears(typeId, stageId, false).subscribe({
      next: (items) => {
        this.years.set(items ?? []);
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
    this.api.getSubjects(typeId, stageId, yearId, false).subscribe({
      next: (items) => {
        this.subjects.set(items ?? []);
        this.loadingSubjects.set(false);
      },
      error: (err) => {
        this.loadingSubjects.set(false);
        this.error.set(this.apiError(err, 'Failed to load subjects.'));
      },
    });
  }

  private sortByOrder<T extends { sortOrder?: number; nameEn?: string }>(items: T[]): T[] {
    return [...items].sort(
      (a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0) || (a.nameEn ?? '').localeCompare(b.nameEn ?? ''),
    );
  }

  private bumpTypeStages(typeId: number, delta: number): void {
    this.types.update((items) =>
      items.map((item) =>
        item.id === typeId
          ? Object.assign(new EducationTypeDto(), item, {
              stagesCount: Math.max(0, (item.stagesCount ?? 0) + delta),
            })
          : item,
      ),
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

  private clearFromType(): void {
    this.selectedTypeId.set(null);
    this.selectedStageId.set(null);
    this.selectedYearId.set(null);
    this.stages.set([]);
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
