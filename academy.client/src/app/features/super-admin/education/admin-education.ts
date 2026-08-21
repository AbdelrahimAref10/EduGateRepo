import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CreateEducationTypeRequest,
  CreateEducationYearRequest,
  EducationTypeDto,
  EducationTypesClient,
  EducationYearDto,
  UpdateEducationTypeRequest,
  UpdateEducationYearRequest,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
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
  private readonly i18n = inject(TranslationService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly loading = signal(false);
  readonly savingType = signal(false);
  readonly savingYear = signal(false);
  readonly loadingYears = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly types = signal<EducationTypeDto[]>([]);
  readonly years = signal<EducationYearDto[]>([]);
  readonly selectedTypeId = signal<number | null>(null);
  readonly editingTypeId = signal<number | null>(null);
  readonly editingYearId = signal<number | null>(null);

  readonly typeForm = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.required, Validators.maxLength(150)]],
    sortOrder: [0, [Validators.required, Validators.min(0)]],
  });

  readonly yearForm = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.required, Validators.maxLength(150)]],
    sortOrder: [0, [Validators.required, Validators.min(0)]],
  });

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
          this.selectedTypeId.set(null);
          this.years.set([]);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load education types.');
      },
    });
  }

  selectType(typeId: number): void {
    this.selectedTypeId.set(typeId);
    this.success.set(null);
    this.error.set(null);
    this.cancelEditYear();
    this.loadYears(typeId);
  }

  loadYears(typeId: number): void {
    this.loadingYears.set(true);

    this.api.getYears(typeId, false).subscribe({
      next: (items) => {
        this.years.set(items ?? []);
        this.loadingYears.set(false);
      },
      error: (err) => {
        this.loadingYears.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load education years.');
      },
    });
  }

  startEditType(type: EducationTypeDto): void {
    this.editingTypeId.set(type.id);
    this.typeForm.setValue({
      nameAr: type.nameAr,
      nameEn: type.nameEn,
      sortOrder: type.sortOrder,
    });
    this.error.set(null);
    this.success.set(null);
  }

  cancelEditType(): void {
    this.editingTypeId.set(null);
    this.typeForm.reset({ nameAr: '', nameEn: '', sortOrder: this.types().length });
  }

  saveType(): void {
    if (this.editingTypeId()) {
      this.updateType();
      return;
    }
    this.createType();
  }

  createType(): void {
    this.error.set(null);
    this.success.set(null);

    if (this.typeForm.invalid) {
      this.typeForm.markAllAsTouched();
      return;
    }

    const value = this.typeForm.getRawValue();
    const request = new CreateEducationTypeRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      sortOrder: value.sortOrder,
    });

    this.savingType.set(true);
    this.api.createType(request).subscribe({
      next: (created) => {
        this.savingType.set(false);
        this.success.set('typeCreated');
        this.typeForm.reset({ nameAr: '', nameEn: '', sortOrder: this.types().length + 1 });
        this.types.update((items) =>
          [...items, created].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0)),
        );
        this.selectType(created.id);
      },
      error: (err) => {
        this.savingType.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to create education type.');
      },
    });
  }

  updateType(): void {
    this.error.set(null);
    this.success.set(null);

    const typeId = this.editingTypeId();
    if (!typeId) return;

    if (this.typeForm.invalid) {
      this.typeForm.markAllAsTouched();
      return;
    }

    const value = this.typeForm.getRawValue();
    const request = new UpdateEducationTypeRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      sortOrder: value.sortOrder,
    });

    this.savingType.set(true);
    this.api.updateType(typeId, request).subscribe({
      next: (updated) => {
        this.savingType.set(false);
        this.success.set('typeUpdated');
        this.editingTypeId.set(null);
        this.typeForm.reset({ nameAr: '', nameEn: '', sortOrder: this.types().length });
        this.types.update((items) =>
          items
            .map((item) => (item.id === updated.id ? updated : item))
            .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0)),
        );
      },
      error: (err) => {
        this.savingType.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to update education type.');
      },
    });
  }

  startEditYear(year: EducationYearDto): void {
    this.editingYearId.set(year.id);
    this.yearForm.setValue({
      nameAr: year.nameAr,
      nameEn: year.nameEn,
      sortOrder: year.sortOrder,
    });
    this.error.set(null);
    this.success.set(null);
  }

  cancelEditYear(): void {
    this.editingYearId.set(null);
    this.yearForm.reset({ nameAr: '', nameEn: '', sortOrder: this.years().length });
  }

  saveYear(): void {
    if (this.editingYearId()) {
      this.updateYear();
      return;
    }
    this.createYear();
  }

  createYear(): void {
    this.error.set(null);
    this.success.set(null);

    const typeId = this.selectedTypeId();
    if (!typeId) {
      this.error.set('Select an education type first.');
      return;
    }

    if (this.yearForm.invalid) {
      this.yearForm.markAllAsTouched();
      return;
    }

    const value = this.yearForm.getRawValue();
    const request = new CreateEducationYearRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      sortOrder: value.sortOrder,
    });

    this.savingYear.set(true);
    this.api.createYear(typeId, request).subscribe({
      next: (created) => {
        this.savingYear.set(false);
        this.success.set('yearCreated');
        this.yearForm.reset({ nameAr: '', nameEn: '', sortOrder: this.years().length + 1 });
        this.years.update((items) =>
          [...items, created].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0)),
        );
        this.types.update((items) =>
          items.map((item) =>
            item.id === typeId
              ? Object.assign(new EducationTypeDto(), item, {
                  yearsCount: (item.yearsCount ?? 0) + 1,
                })
              : item,
          ),
        );
      },
      error: (err) => {
        this.savingYear.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to create education year.');
      },
    });
  }

  updateYear(): void {
    this.error.set(null);
    this.success.set(null);

    const typeId = this.selectedTypeId();
    const yearId = this.editingYearId();
    if (!typeId || !yearId) return;

    if (this.yearForm.invalid) {
      this.yearForm.markAllAsTouched();
      return;
    }

    const value = this.yearForm.getRawValue();
    const request = new UpdateEducationYearRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      sortOrder: value.sortOrder,
    });

    this.savingYear.set(true);
    this.api.updateYear(typeId, yearId, request).subscribe({
      next: (updated) => {
        this.savingYear.set(false);
        this.success.set('yearUpdated');
        this.editingYearId.set(null);
        this.yearForm.reset({ nameAr: '', nameEn: '', sortOrder: this.years().length });
        this.years.update((items) =>
          items
            .map((item) => (item.id === updated.id ? updated : item))
            .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0)),
        );
      },
      error: (err) => {
        this.savingYear.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to update education year.');
      },
    });
  }

  deleteType(type: EducationTypeDto): void {
    void this.runDeleteType(type);
  }

  private async runDeleteType(type: EducationTypeDto): Promise<void> {
    const ok = await this.confirmDialog.ask({
      messageKey: 'education.confirmDeleteType',
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.error.set(null);
    this.success.set(null);
    this.deletingId.set(`type-${type.id}`);

    this.api.deleteType(type.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('typeDeleted');
        this.types.update((items) => items.filter((item) => item.id !== type.id));
        if (this.selectedTypeId() === type.id) {
          this.selectedTypeId.set(null);
          this.years.set([]);
        }
        if (this.editingTypeId() === type.id) this.cancelEditType();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to delete education type.');
      },
    });
  }

  deleteYear(year: EducationYearDto): void {
    void this.runDeleteYear(year);
  }

  private async runDeleteYear(year: EducationYearDto): Promise<void> {
    const typeId = this.selectedTypeId();
    if (!typeId) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'education.confirmDeleteYear',
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.error.set(null);
    this.success.set(null);
    this.deletingId.set(`year-${year.id}`);

    this.api.deleteYear(typeId, year.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('yearDeleted');
        this.years.update((items) => items.filter((item) => item.id !== year.id));
        this.types.update((items) =>
          items.map((item) =>
            item.id === typeId
              ? Object.assign(new EducationTypeDto(), item, {
                  yearsCount: Math.max(0, (item.yearsCount ?? 0) - 1),
                })
              : item,
          ),
        );
        if (this.editingYearId() === year.id) this.cancelEditYear();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to delete education year.');
      },
    });
  }

  label(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
  }

  selectedTypeName(): string {
    const type = this.types().find((item) => item.id === this.selectedTypeId());
    return type ? type.name : '';
  }
}
