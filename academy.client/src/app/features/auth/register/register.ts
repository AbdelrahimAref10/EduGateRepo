import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  AppRole,
  AreaDto,
  CitiesClient,
  CityDto,
  CountriesClient,
  CountryDto,
  GovernorateDto,
  GovernoratesClient,
} from '../../../core/api/academy-api.generated';
import { AuthService } from '../../../core/auth/auth.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { LanguageSwitcherComponent } from '../../../shared/language-switcher/language-switcher';

interface RoleOption {
  value: AppRole;
  labelKey: string;
  hintKey: string;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly countriesApi = inject(CountriesClient);
  private readonly governoratesApi = inject(GovernoratesClient);
  private readonly citiesApi = inject(CitiesClient);
  private readonly i18n = inject(TranslationService);

  readonly loading = signal(false);
  readonly loadingCountries = signal(false);
  readonly loadingGovernorates = signal(false);
  readonly loadingCities = signal(false);
  readonly loadingAreas = signal(false);
  readonly error = signal<string | null>(null);

  readonly countries = signal<CountryDto[]>([]);
  readonly governorates = signal<GovernorateDto[]>([]);
  readonly cities = signal<CityDto[]>([]);
  readonly areas = signal<AreaDto[]>([]);

  readonly roles: RoleOption[] = [
    { value: AppRole.Teacher, labelKey: 'auth.roleTeacher', hintKey: 'auth.roleTeacherHint' },
    { value: AppRole.Student, labelKey: 'auth.roleStudent', hintKey: 'auth.roleStudentHint' },
    { value: AppRole.Parent, labelKey: 'auth.roleParent', hintKey: 'auth.roleParentHint' },
  ];

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],
    role: [AppRole.Student as AppRole, [Validators.required]],
    countryId: [null as number | null, [Validators.required]],
    governorateId: [null as number | null, [Validators.required]],
    cityId: [null as number | null, [Validators.required]],
    areaId: [null as number | null, [Validators.required]],
  });

  ngOnInit(): void {
    this.loadCountries();

    this.form.controls.countryId.valueChanges.subscribe((countryId) => {
      this.form.controls.governorateId.setValue(null);
      this.form.controls.cityId.setValue(null);
      this.form.controls.areaId.setValue(null);
      this.governorates.set([]);
      this.cities.set([]);
      this.areas.set([]);
      if (countryId) this.loadGovernorates(countryId);
    });

    this.form.controls.governorateId.valueChanges.subscribe((governorateId) => {
      this.form.controls.cityId.setValue(null);
      this.form.controls.areaId.setValue(null);
      this.cities.set([]);
      this.areas.set([]);
      if (governorateId) this.loadCities(governorateId);
    });

    this.form.controls.cityId.valueChanges.subscribe((cityId) => {
      this.form.controls.areaId.setValue(null);
      this.areas.set([]);
      if (cityId) this.loadAreas(cityId);
    });
  }

  selectRole(role: AppRole): void {
    this.form.controls.role.setValue(role);
  }

  placeLabel(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
  }

  submit(): void {
    this.error.set(null);
    const value = this.form.getRawValue();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (value.password !== value.confirmPassword) {
      this.error.set('Password and confirm password do not match.');
      return;
    }

    if (!value.areaId) {
      this.error.set('Please select an area.');
      return;
    }

    this.loading.set(true);
    this.auth
      .register({
        email: value.email,
        password: value.password,
        confirmPassword: value.confirmPassword,
        firstName: value.firstName,
        lastName: value.lastName,
        phoneNumber: value.phoneNumber || undefined,
        role: value.role,
        areaId: value.areaId,
      })
      .subscribe({
        next: () => this.loading.set(false),
        error: (err) => {
          this.loading.set(false);
          this.error.set(err?.error?.detail || err?.error?.title || 'Unable to create account.');
        },
      });
  }

  private loadCountries(): void {
    this.loadingCountries.set(true);
    this.countriesApi.getCountries(true).subscribe({
      next: (items) => {
        this.countries.set(items ?? []);
        this.loadingCountries.set(false);
      },
      error: (err) => {
        this.loadingCountries.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load countries.');
      },
    });
  }

  private loadGovernorates(countryId: number): void {
    this.loadingGovernorates.set(true);
    this.countriesApi.getGovernorates(countryId, true).subscribe({
      next: (items) => {
        this.governorates.set(items ?? []);
        this.loadingGovernorates.set(false);
      },
      error: (err) => {
        this.loadingGovernorates.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load governorates.');
      },
    });
  }

  private loadCities(governorateId: number): void {
    this.loadingCities.set(true);
    this.governoratesApi.getCities(governorateId, true).subscribe({
      next: (items) => {
        this.cities.set(items ?? []);
        this.loadingCities.set(false);
      },
      error: (err) => {
        this.loadingCities.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load cities.');
      },
    });
  }

  private loadAreas(cityId: number): void {
    this.loadingAreas.set(true);
    this.citiesApi.getAreas(cityId, true).subscribe({
      next: (items) => {
        this.areas.set(items ?? []);
        this.loadingAreas.set(false);
      },
      error: (err) => {
        this.loadingAreas.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load areas.');
      },
    });
  }
}
