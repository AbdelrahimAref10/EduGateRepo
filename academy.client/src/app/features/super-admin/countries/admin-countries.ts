import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AreaDto,
  CitiesClient,
  CityDto,
  CountriesClient,
  CountryDto,
  CreateCountryRequest,
  CreateLocationNameRequest,
  GovernorateDto,
  GovernoratesClient,
} from '../../../core/api/academy-api.generated';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-admin-countries',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './admin-countries.html',
  styleUrl: './admin-countries.css',
})
export class AdminCountriesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly countriesApi = inject(CountriesClient);
  private readonly governoratesApi = inject(GovernoratesClient);
  private readonly citiesApi = inject(CitiesClient);
  private readonly i18n = inject(TranslationService);

  readonly loading = signal(false);
  readonly loadingGovernorates = signal(false);
  readonly loadingCities = signal(false);
  readonly loadingAreas = signal(false);
  readonly savingCountry = signal(false);
  readonly savingGovernorate = signal(false);
  readonly savingCity = signal(false);
  readonly savingArea = signal(false);
  readonly deletingId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly countries = signal<CountryDto[]>([]);
  readonly governorates = signal<GovernorateDto[]>([]);
  readonly cities = signal<CityDto[]>([]);
  readonly areas = signal<AreaDto[]>([]);

  readonly selectedCountryId = signal<number | null>(null);
  readonly selectedGovernorateId = signal<number | null>(null);
  readonly selectedCityId = signal<number | null>(null);

  readonly editingCountryId = signal<number | null>(null);
  readonly editingGovernorateId = signal<number | null>(null);
  readonly editingCityId = signal<number | null>(null);
  readonly editingAreaId = signal<number | null>(null);

  readonly countryForm = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.required, Validators.maxLength(150)]],
    code: ['', [Validators.required, Validators.maxLength(10)]],
  });

  readonly governorateForm = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.required, Validators.maxLength(150)]],
  });

  readonly cityForm = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.required, Validators.maxLength(150)]],
  });

  readonly areaForm = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(150)]],
    nameEn: ['', [Validators.required, Validators.maxLength(150)]],
  });

  ngOnInit(): void {
    this.loadCountries();
  }

  loadCountries(): void {
    this.loading.set(true);
    this.error.set(null);

    this.countriesApi.getCountries(false).subscribe({
      next: (items) => {
        this.countries.set(items ?? []);
        this.loading.set(false);

        const selected = this.selectedCountryId();
        if (selected && !(items ?? []).some((item) => item.id === selected)) {
          this.clearFromCountry();
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.apiError(err, 'Failed to load countries.'));
      },
    });
  }

  selectCountry(countryId: number): void {
    this.selectedCountryId.set(countryId);
    this.selectedGovernorateId.set(null);
    this.selectedCityId.set(null);
    this.governorates.set([]);
    this.cities.set([]);
    this.areas.set([]);
    this.cancelEditGovernorate();
    this.cancelEditCity();
    this.cancelEditArea();
    this.success.set(null);
    this.error.set(null);
    this.loadGovernorates(countryId);
  }

  selectGovernorate(governorateId: number): void {
    this.selectedGovernorateId.set(governorateId);
    this.selectedCityId.set(null);
    this.cities.set([]);
    this.areas.set([]);
    this.cancelEditCity();
    this.cancelEditArea();
    this.success.set(null);
    this.error.set(null);
    this.loadCities(governorateId);
  }

  selectCity(cityId: number): void {
    this.selectedCityId.set(cityId);
    this.areas.set([]);
    this.cancelEditArea();
    this.success.set(null);
    this.error.set(null);
    this.loadAreas(cityId);
  }

  startEditCountry(country: CountryDto): void {
    this.editingCountryId.set(country.id);
    this.countryForm.setValue({
      nameAr: country.nameAr,
      nameEn: country.nameEn,
      code: country.code,
    });
    this.error.set(null);
    this.success.set(null);
  }

  cancelEditCountry(): void {
    this.editingCountryId.set(null);
    this.countryForm.reset({ nameAr: '', nameEn: '', code: '' });
  }

  saveCountry(): void {
    if (this.editingCountryId()) {
      this.updateCountry();
      return;
    }
    this.createCountry();
  }

  createCountry(): void {
    this.error.set(null);
    this.success.set(null);

    if (this.countryForm.invalid) {
      this.countryForm.markAllAsTouched();
      return;
    }

    const value = this.countryForm.getRawValue();
    const request = new CreateCountryRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      code: value.code.trim(),
    });

    this.savingCountry.set(true);
    this.countriesApi.createCountry(request).subscribe({
      next: (created) => {
        this.savingCountry.set(false);
        this.success.set('countryCreated');
        this.countryForm.reset({ nameAr: '', nameEn: '', code: '' });
        this.loadCountries();
        if (created.id) this.selectCountry(created.id);
      },
      error: (err) => {
        this.savingCountry.set(false);
        this.error.set(this.apiError(err, 'Failed to create country.'));
      },
    });
  }

  updateCountry(): void {
    this.error.set(null);
    this.success.set(null);

    const countryId = this.editingCountryId();
    if (!countryId) return;

    if (this.countryForm.invalid) {
      this.countryForm.markAllAsTouched();
      return;
    }

    const value = this.countryForm.getRawValue();
    const request = new CreateCountryRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      code: value.code.trim(),
    });

    this.savingCountry.set(true);
    this.countriesApi.updateCountry(countryId, request).subscribe({
      next: () => {
        this.savingCountry.set(false);
        this.success.set('countryUpdated');
        this.cancelEditCountry();
        this.loadCountries();
      },
      error: (err) => {
        this.savingCountry.set(false);
        this.error.set(this.apiError(err, 'Failed to update country.'));
      },
    });
  }

  deleteCountry(country: CountryDto): void {
    if (!confirm(this.i18n.t('countries.confirmDeleteCountry'))) return;

    this.error.set(null);
    this.success.set(null);
    this.deletingId.set(`country-${country.id}`);

    this.countriesApi.deleteCountry(country.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('countryDeleted');
        if (this.selectedCountryId() === country.id) this.clearFromCountry();
        if (this.editingCountryId() === country.id) this.cancelEditCountry();
        this.loadCountries();
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete country.'));
      },
    });
  }

  startEditGovernorate(gov: GovernorateDto): void {
    this.editingGovernorateId.set(gov.id);
    this.governorateForm.setValue({ nameAr: gov.nameAr, nameEn: gov.nameEn });
    this.error.set(null);
    this.success.set(null);
  }

  cancelEditGovernorate(): void {
    this.editingGovernorateId.set(null);
    this.governorateForm.reset({ nameAr: '', nameEn: '' });
  }

  saveGovernorate(): void {
    if (this.editingGovernorateId()) {
      this.updateGovernorate();
      return;
    }
    this.createGovernorate();
  }

  createGovernorate(): void {
    this.error.set(null);
    this.success.set(null);

    const countryId = this.selectedCountryId();
    if (!countryId) {
      this.error.set('Select a country first.');
      return;
    }

    if (this.governorateForm.invalid) {
      this.governorateForm.markAllAsTouched();
      return;
    }

    const value = this.governorateForm.getRawValue();
    const request = new CreateLocationNameRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
    });

    this.savingGovernorate.set(true);
    this.countriesApi.createGovernorate(countryId, request).subscribe({
      next: (created) => {
        this.savingGovernorate.set(false);
        this.success.set('governorateCreated');
        this.governorateForm.reset({ nameAr: '', nameEn: '' });
        this.loadCountries();
        this.loadGovernorates(countryId);
        if (created.id) this.selectGovernorate(created.id);
      },
      error: (err) => {
        this.savingGovernorate.set(false);
        this.error.set(this.apiError(err, 'Failed to create governorate.'));
      },
    });
  }

  updateGovernorate(): void {
    this.error.set(null);
    this.success.set(null);

    const countryId = this.selectedCountryId();
    const governorateId = this.editingGovernorateId();
    if (!countryId || !governorateId) return;

    if (this.governorateForm.invalid) {
      this.governorateForm.markAllAsTouched();
      return;
    }

    const value = this.governorateForm.getRawValue();
    const request = new CreateLocationNameRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
    });

    this.savingGovernorate.set(true);
    this.countriesApi.updateGovernorate(countryId, governorateId, request).subscribe({
      next: () => {
        this.savingGovernorate.set(false);
        this.success.set('governorateUpdated');
        this.cancelEditGovernorate();
        this.loadGovernorates(countryId);
      },
      error: (err) => {
        this.savingGovernorate.set(false);
        this.error.set(this.apiError(err, 'Failed to update governorate.'));
      },
    });
  }

  deleteGovernorate(gov: GovernorateDto): void {
    const countryId = this.selectedCountryId();
    if (!countryId) return;
    if (!confirm(this.i18n.t('countries.confirmDeleteGovernorate'))) return;

    this.error.set(null);
    this.success.set(null);
    this.deletingId.set(`gov-${gov.id}`);

    this.countriesApi.deleteGovernorate(countryId, gov.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('governorateDeleted');
        if (this.selectedGovernorateId() === gov.id) {
          this.selectedGovernorateId.set(null);
          this.selectedCityId.set(null);
          this.cities.set([]);
          this.areas.set([]);
        }
        if (this.editingGovernorateId() === gov.id) this.cancelEditGovernorate();
        this.loadCountries();
        this.loadGovernorates(countryId);
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete governorate.'));
      },
    });
  }

  startEditCity(city: CityDto): void {
    this.editingCityId.set(city.id);
    this.cityForm.setValue({ nameAr: city.nameAr, nameEn: city.nameEn });
    this.error.set(null);
    this.success.set(null);
  }

  cancelEditCity(): void {
    this.editingCityId.set(null);
    this.cityForm.reset({ nameAr: '', nameEn: '' });
  }

  saveCity(): void {
    if (this.editingCityId()) {
      this.updateCity();
      return;
    }
    this.createCity();
  }

  createCity(): void {
    this.error.set(null);
    this.success.set(null);

    const governorateId = this.selectedGovernorateId();
    if (!governorateId) {
      this.error.set('Select a governorate first.');
      return;
    }

    if (this.cityForm.invalid) {
      this.cityForm.markAllAsTouched();
      return;
    }

    const value = this.cityForm.getRawValue();
    const request = new CreateLocationNameRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
    });

    this.savingCity.set(true);
    this.governoratesApi.createCity(governorateId, request).subscribe({
      next: (created) => {
        this.savingCity.set(false);
        this.success.set('cityCreated');
        this.cityForm.reset({ nameAr: '', nameEn: '' });
        const countryId = this.selectedCountryId();
        if (countryId) this.loadGovernorates(countryId);
        this.loadCities(governorateId);
        if (created.id) this.selectCity(created.id);
      },
      error: (err) => {
        this.savingCity.set(false);
        this.error.set(this.apiError(err, 'Failed to create city.'));
      },
    });
  }

  updateCity(): void {
    this.error.set(null);
    this.success.set(null);

    const governorateId = this.selectedGovernorateId();
    const cityId = this.editingCityId();
    if (!governorateId || !cityId) return;

    if (this.cityForm.invalid) {
      this.cityForm.markAllAsTouched();
      return;
    }

    const value = this.cityForm.getRawValue();
    const request = new CreateLocationNameRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
    });

    this.savingCity.set(true);
    this.governoratesApi.updateCity(governorateId, cityId, request).subscribe({
      next: () => {
        this.savingCity.set(false);
        this.success.set('cityUpdated');
        this.cancelEditCity();
        this.loadCities(governorateId);
      },
      error: (err) => {
        this.savingCity.set(false);
        this.error.set(this.apiError(err, 'Failed to update city.'));
      },
    });
  }

  deleteCity(city: CityDto): void {
    const governorateId = this.selectedGovernorateId();
    if (!governorateId) return;
    if (!confirm(this.i18n.t('countries.confirmDeleteCity'))) return;

    this.error.set(null);
    this.success.set(null);
    this.deletingId.set(`city-${city.id}`);

    this.governoratesApi.deleteCity(governorateId, city.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('cityDeleted');
        if (this.selectedCityId() === city.id) {
          this.selectedCityId.set(null);
          this.areas.set([]);
        }
        if (this.editingCityId() === city.id) this.cancelEditCity();
        const countryId = this.selectedCountryId();
        if (countryId) this.loadGovernorates(countryId);
        this.loadCities(governorateId);
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete city.'));
      },
    });
  }

  startEditArea(area: AreaDto): void {
    this.editingAreaId.set(area.id);
    this.areaForm.setValue({ nameAr: area.nameAr, nameEn: area.nameEn });
    this.error.set(null);
    this.success.set(null);
  }

  cancelEditArea(): void {
    this.editingAreaId.set(null);
    this.areaForm.reset({ nameAr: '', nameEn: '' });
  }

  saveArea(): void {
    if (this.editingAreaId()) {
      this.updateArea();
      return;
    }
    this.createArea();
  }

  createArea(): void {
    this.error.set(null);
    this.success.set(null);

    const cityId = this.selectedCityId();
    if (!cityId) {
      this.error.set('Select a city first.');
      return;
    }

    if (this.areaForm.invalid) {
      this.areaForm.markAllAsTouched();
      return;
    }

    const value = this.areaForm.getRawValue();
    const request = new CreateLocationNameRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
    });

    this.savingArea.set(true);
    this.citiesApi.createArea(cityId, request).subscribe({
      next: () => {
        this.savingArea.set(false);
        this.success.set('areaCreated');
        this.areaForm.reset({ nameAr: '', nameEn: '' });
        const governorateId = this.selectedGovernorateId();
        if (governorateId) this.loadCities(governorateId);
        this.loadAreas(cityId);
      },
      error: (err) => {
        this.savingArea.set(false);
        this.error.set(this.apiError(err, 'Failed to create area.'));
      },
    });
  }

  updateArea(): void {
    this.error.set(null);
    this.success.set(null);

    const cityId = this.selectedCityId();
    const areaId = this.editingAreaId();
    if (!cityId || !areaId) return;

    if (this.areaForm.invalid) {
      this.areaForm.markAllAsTouched();
      return;
    }

    const value = this.areaForm.getRawValue();
    const request = new CreateLocationNameRequest({
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
    });

    this.savingArea.set(true);
    this.citiesApi.updateArea(cityId, areaId, request).subscribe({
      next: () => {
        this.savingArea.set(false);
        this.success.set('areaUpdated');
        this.cancelEditArea();
        this.loadAreas(cityId);
      },
      error: (err) => {
        this.savingArea.set(false);
        this.error.set(this.apiError(err, 'Failed to update area.'));
      },
    });
  }

  deleteArea(area: AreaDto): void {
    const cityId = this.selectedCityId();
    if (!cityId) return;
    if (!confirm(this.i18n.t('countries.confirmDeleteArea'))) return;

    this.error.set(null);
    this.success.set(null);
    this.deletingId.set(`area-${area.id}`);

    this.citiesApi.deleteArea(cityId, area.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.success.set('areaDeleted');
        if (this.editingAreaId() === area.id) this.cancelEditArea();
        const governorateId = this.selectedGovernorateId();
        if (governorateId) this.loadCities(governorateId);
        this.loadAreas(cityId);
      },
      error: (err) => {
        this.deletingId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete area.'));
      },
    });
  }

  selectedCountryName(): string {
    const item = this.countries().find((x) => x.id === this.selectedCountryId());
    return item ? item.name : '';
  }

  selectedGovernorateName(): string {
    const item = this.governorates().find((x) => x.id === this.selectedGovernorateId());
    return item ? item.name : '';
  }

  selectedCityName(): string {
    const item = this.cities().find((x) => x.id === this.selectedCityId());
    return item ? item.name : '';
  }

  label(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
  }

  private loadGovernorates(countryId: number): void {
    this.loadingGovernorates.set(true);
    this.countriesApi.getGovernorates(countryId, false).subscribe({
      next: (items) => {
        this.governorates.set(items ?? []);
        this.loadingGovernorates.set(false);
      },
      error: (err) => {
        this.loadingGovernorates.set(false);
        this.error.set(this.apiError(err, 'Failed to load governorates.'));
      },
    });
  }

  private loadCities(governorateId: number): void {
    this.loadingCities.set(true);
    this.governoratesApi.getCities(governorateId, false).subscribe({
      next: (items) => {
        this.cities.set(items ?? []);
        this.loadingCities.set(false);
      },
      error: (err) => {
        this.loadingCities.set(false);
        this.error.set(this.apiError(err, 'Failed to load cities.'));
      },
    });
  }

  private loadAreas(cityId: number): void {
    this.loadingAreas.set(true);
    this.citiesApi.getAreas(cityId, false).subscribe({
      next: (items) => {
        this.areas.set(items ?? []);
        this.loadingAreas.set(false);
      },
      error: (err) => {
        this.loadingAreas.set(false);
        this.error.set(this.apiError(err, 'Failed to load areas.'));
      },
    });
  }

  private clearFromCountry(): void {
    this.selectedCountryId.set(null);
    this.selectedGovernorateId.set(null);
    this.selectedCityId.set(null);
    this.governorates.set([]);
    this.cities.set([]);
    this.areas.set([]);
  }

  private apiError(err: any, fallback: string): string {
    return err?.result?.detail || err?.message || fallback;
  }
}
