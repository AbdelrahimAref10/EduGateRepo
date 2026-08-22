import { signal } from '@angular/core';
import {
  CountriesClient,
  CountryDto,
  EducationStageDto,
  EducationSubjectDto,
  EducationTypeDto,
  EducationTypesClient,
  EducationYearDto,
  PublicMarketplaceClient,
  PublicTeacherListItemDto,
} from '../../core/api/academy-api.generated';
import { TranslationService } from '../../core/i18n/translation.service';

export class MarketplaceCatalog {
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly teachers = signal<PublicTeacherListItemDto[]>([]);
  readonly countries = signal<CountryDto[]>([]);
  readonly types = signal<EducationTypeDto[]>([]);
  readonly stages = signal<EducationStageDto[]>([]);
  readonly years = signal<EducationYearDto[]>([]);
  readonly subjects = signal<EducationSubjectDto[]>([]);
  readonly countryId = signal<number | null>(null);
  readonly typeId = signal<number | null>(null);
  readonly stageId = signal<number | null>(null);
  readonly yearId = signal<number | null>(null);
  readonly subjectId = signal<number | null>(null);

  constructor(
    private readonly marketplaceApi: PublicMarketplaceClient,
    private readonly countriesApi: CountriesClient,
    private readonly educationApi: EducationTypesClient,
    private readonly i18n: TranslationService,
  ) {}

  init(): void {
    this.countriesApi.getCountries(true).subscribe({
      next: (items) => this.countries.set(items ?? []),
    });
    this.educationApi.getTypes(true).subscribe({
      next: (items) => this.types.set(items ?? []),
    });
    this.loadTeachers();
  }

  loadTeachers(): void {
    this.loading.set(true);
    this.error.set(null);
    this.marketplaceApi
      .getTeachers(this.countryId(), this.stageId(), this.subjectId())
      .subscribe({
        next: (items) => {
          this.teachers.set(items ?? []);
          this.loading.set(false);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to load teachers.');
        },
      });
  }

  onCountry(value: string): void {
    this.countryId.set(value ? Number(value) : null);
    this.loadTeachers();
  }

  onType(value: string): void {
    const id = value ? Number(value) : null;
    this.typeId.set(id);
    this.stageId.set(null);
    this.yearId.set(null);
    this.subjectId.set(null);
    this.stages.set([]);
    this.years.set([]);
    this.subjects.set([]);
    if (id) {
      this.educationApi.getStages(id, true).subscribe({
        next: (items) => this.stages.set(items ?? []),
      });
    }
    this.loadTeachers();
  }

  onStage(value: string): void {
    const id = value ? Number(value) : null;
    this.stageId.set(id);
    this.yearId.set(null);
    this.subjectId.set(null);
    this.years.set([]);
    this.subjects.set([]);
    const typeId = this.typeId();
    if (id && typeId) {
      this.educationApi.getYears(typeId, id, true).subscribe({
        next: (items) => this.years.set(items ?? []),
      });
    }
    this.loadTeachers();
  }

  onYear(value: string): void {
    const id = value ? Number(value) : null;
    this.yearId.set(id);
    this.subjectId.set(null);
    this.subjects.set([]);
    const typeId = this.typeId();
    const stageId = this.stageId();
    if (id && typeId && stageId) {
      this.educationApi.getSubjects(typeId, stageId, id, true).subscribe({
        next: (items) => this.subjects.set(items ?? []),
      });
    }
  }

  onSubject(value: string): void {
    this.subjectId.set(value ? Number(value) : null);
    this.loadTeachers();
  }

  placeLabel(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
  }
}
