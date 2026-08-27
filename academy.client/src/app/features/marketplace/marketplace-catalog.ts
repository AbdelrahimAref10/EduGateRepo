import { signal } from '@angular/core';
import {
  AcademicYearDto,
  AcademicYearsClient,
  CountriesClient,
  CountryDto,
  EducationStageDto,
  EducationStagesClient,
  EducationSubjectDto,
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
  readonly academicYears = signal<AcademicYearDto[]>([]);
  readonly stages = signal<EducationStageDto[]>([]);
  readonly years = signal<EducationYearDto[]>([]);
  readonly subjects = signal<EducationSubjectDto[]>([]);
  readonly countryId = signal<number | null>(null);
  readonly academicYearId = signal<number | null>(null);
  readonly stageId = signal<number | null>(null);
  readonly yearId = signal<number | null>(null);
  readonly subjectId = signal<number | null>(null);

  constructor(
    private readonly marketplaceApi: PublicMarketplaceClient,
    private readonly countriesApi: CountriesClient,
    private readonly academicYearsApi: AcademicYearsClient,
    private readonly stagesApi: EducationStagesClient,
    private readonly i18n: TranslationService,
  ) {}

  init(): void {
    this.countriesApi.getCountries(true).subscribe({
      next: (items) => this.countries.set(items ?? []),
    });
    this.academicYearsApi.get(true).subscribe({
      next: (items) => this.academicYears.set(items ?? []),
    });
    this.stagesApi.getStages(true).subscribe({
      next: (items) => this.stages.set(items ?? []),
    });
    this.loadTeachers();
  }

  loadTeachers(): void {
    this.loading.set(true);
    this.error.set(null);
    this.marketplaceApi
      .getTeachers(this.countryId(), this.academicYearId(), this.stageId(), this.subjectId())
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

  onAcademicYear(value: string): void {
    this.academicYearId.set(value ? Number(value) : null);
    this.loadTeachers();
  }

  onStage(value: string): void {
    const id = value ? Number(value) : null;
    this.stageId.set(id);
    this.yearId.set(null);
    this.subjectId.set(null);
    this.years.set([]);
    this.subjects.set([]);
    if (id) {
      this.stagesApi.getYears(id, true).subscribe({
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
    const stageId = this.stageId();
    if (id && stageId) {
      this.stagesApi.getSubjects(stageId, id, true).subscribe({
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
