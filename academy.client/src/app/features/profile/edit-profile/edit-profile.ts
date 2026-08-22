import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AccountClient,
  AreaDto,
  CitiesClient,
  CityDto,
  CountriesClient,
  CountryDto,
  GovernorateDto,
  GovernoratesClient,
  UpdateMyProfileRequest,
  UserProfileDto,
} from '../../../core/api/academy-api.generated';
import { AuthService } from '../../../core/auth/auth.service';
import { ImageService } from '../../../core/images/image.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, UserAvatarComponent],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
})
export class EditProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountApi = inject(AccountClient);
  private readonly images = inject(ImageService);
  private readonly countriesApi = inject(CountriesClient);
  private readonly governoratesApi = inject(GovernoratesClient);
  private readonly citiesApi = inject(CitiesClient);
  private readonly auth = inject(AuthService);
  private readonly i18n = inject(TranslationService);

  private hydrating = false;

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly loadingCountries = signal(false);
  readonly loadingGovernorates = signal(false);
  readonly loadingCities = signal(false);
  readonly loadingAreas = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal(false);
  readonly photoDraft = signal<string | null | undefined>(undefined);

  readonly profile = signal<UserProfileDto | null>(null);
  readonly countries = signal<CountryDto[]>([]);
  readonly governorates = signal<GovernorateDto[]>([]);
  readonly cities = signal<CityDto[]>([]);
  readonly areas = signal<AreaDto[]>([]);

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    bio: ['', [Validators.maxLength(500)]],
    countryId: [null as number | null, [Validators.required]],
    governorateId: [null as number | null, [Validators.required]],
    cityId: [null as number | null, [Validators.required]],
    areaId: [null as number | null, [Validators.required]],
    currentPassword: [''],
    newPassword: [''],
    confirmNewPassword: [''],
  });

  ngOnInit(): void {
    this.bindLocationCascade();
    void this.init();
  }

  previewPhoto(): string | null {
    const draft = this.photoDraft();
    if (draft === undefined) return this.profile()?.photoUrl ?? null;
    return draft;
  }

  async onPhotoSelected(event: Event): Promise<void> {
    this.error.set(null);
    this.success.set(false);
    try {
      this.photoDraft.set(await this.images.fromPicker(event));
    } catch {
      this.error.set(this.i18n.t('profile.photoInvalid'));
    }
  }

  removePhoto(): void {
    this.photoDraft.set(null);
    this.success.set(false);
  }

  placeLabel(ar?: string, en?: string): string {
    return this.i18n.language() === 'ar' ? ar || en || '' : en || ar || '';
  }

  roleLabel(role: string): string {
    switch (role) {
      case 'SuperAdmin':
        return this.i18n.t('auth.roleAdmin');
      case 'Teacher':
        return this.i18n.t('auth.roleTeacher');
      case 'Student':
        return this.i18n.t('auth.roleStudent');
      case 'Parent':
        return this.i18n.t('auth.roleParent');
      default:
        return role;
    }
  }

  submit(): void {
    this.error.set(null);
    this.success.set(false);

    const value = this.form.getRawValue();
    if (this.form.invalid || !value.areaId) {
      this.form.markAllAsTouched();
      return;
    }

    const currentPassword = value.currentPassword.trim();
    const newPassword = value.newPassword.trim();
    const confirmNewPassword = value.confirmNewPassword.trim();

    // Only treat as password change when new/confirm are filled.
    // Browsers often autofill currentPassword alone — that must not block profile save.
    const changingPassword = !!newPassword || !!confirmNewPassword;

    if (changingPassword) {
      if (!currentPassword || !newPassword || !confirmNewPassword) {
        this.error.set(this.i18n.t('profile.passwordRequired'));
        return;
      }
      if (newPassword.length < 6) {
        this.error.set(this.i18n.t('profile.passwordMin'));
        return;
      }
      if (newPassword !== confirmNewPassword) {
        this.error.set(this.i18n.t('profile.passwordMismatch'));
        return;
      }
    }

    const request = new UpdateMyProfileRequest({
      firstName: value.firstName.trim(),
      lastName: value.lastName.trim(),
      email: value.email.trim(),
      phoneNumber: value.phoneNumber.trim() || undefined,
      bio: value.bio.trim() || undefined,
      photoBase64: this.previewPhoto() ?? '',
      areaId: value.areaId,
      currentPassword: changingPassword ? currentPassword : undefined,
      newPassword: changingPassword ? newPassword : undefined,
      confirmNewPassword: changingPassword ? confirmNewPassword : undefined,
    });

    this.saving.set(true);
    this.accountApi.updateMyProfile(request).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.profile.set(updated);
        this.photoDraft.set(undefined);
        this.success.set(true);
        this.form.patchValue({
          currentPassword: '',
          newPassword: '',
          confirmNewPassword: '',
        });
        this.auth.patchSessionIdentity({
          email: updated.email,
          fullName: `${updated.firstName} ${updated.lastName}`.trim(),
          photoUrl: updated.photoUrl ?? null,
        });
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to update profile.');
      },
    });
  }

  private async init(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      await this.loadCountriesAsync();
      const profile = await this.loadProfileAsync();
      this.profile.set(profile);
      await this.hydrateForm(profile);
    } catch (err: any) {
      this.error.set(err?.result?.detail || err?.message || 'Failed to load profile.');
    } finally {
      this.loading.set(false);
    }
  }

  private loadProfileAsync(): Promise<UserProfileDto> {
    return new Promise((resolve, reject) => {
      this.accountApi.getMyProfile().subscribe({
        next: (profile) => resolve(profile),
        error: (err) => reject(err),
      });
    });
  }

  private async hydrateForm(profile: UserProfileDto): Promise<void> {
    this.hydrating = true;

    this.form.patchValue({
      firstName: profile.firstName ?? '',
      lastName: profile.lastName ?? '',
      email: profile.email ?? '',
      phoneNumber: profile.phoneNumber ?? '',
      bio: profile.bio ?? '',
      countryId: null,
      governorateId: null,
      cityId: null,
      areaId: null,
      currentPassword: '',
      newPassword: '',
      confirmNewPassword: '',
    });

    this.governorates.set([]);
    this.cities.set([]);
    this.areas.set([]);

    if (profile.countryId) {
      await this.loadGovernoratesAsync(profile.countryId);
      this.form.controls.countryId.setValue(profile.countryId, { emitEvent: false });
    }

    if (profile.governorateId) {
      await this.loadCitiesAsync(profile.governorateId);
      this.form.controls.governorateId.setValue(profile.governorateId, { emitEvent: false });
    }

    if (profile.cityId) {
      await this.loadAreasAsync(profile.cityId);
      this.form.controls.cityId.setValue(profile.cityId, { emitEvent: false });
    }

    if (profile.areaId) {
      this.form.controls.areaId.setValue(profile.areaId, { emitEvent: false });
    }

    this.hydrating = false;
  }

  private bindLocationCascade(): void {
    this.form.controls.countryId.valueChanges.subscribe((countryId) => {
      if (this.hydrating) return;
      this.form.controls.governorateId.setValue(null);
      this.form.controls.cityId.setValue(null);
      this.form.controls.areaId.setValue(null);
      this.governorates.set([]);
      this.cities.set([]);
      this.areas.set([]);
      if (countryId) void this.loadGovernoratesAsync(countryId);
    });

    this.form.controls.governorateId.valueChanges.subscribe((governorateId) => {
      if (this.hydrating) return;
      this.form.controls.cityId.setValue(null);
      this.form.controls.areaId.setValue(null);
      this.cities.set([]);
      this.areas.set([]);
      if (governorateId) void this.loadCitiesAsync(governorateId);
    });

    this.form.controls.cityId.valueChanges.subscribe((cityId) => {
      if (this.hydrating) return;
      this.form.controls.areaId.setValue(null);
      this.areas.set([]);
      if (cityId) void this.loadAreasAsync(cityId);
    });
  }

  private loadCountriesAsync(): Promise<void> {
    this.loadingCountries.set(true);
    return new Promise((resolve, reject) => {
      this.countriesApi.getCountries(false).subscribe({
        next: (items) => {
          this.countries.set(items ?? []);
          this.loadingCountries.set(false);
          resolve();
        },
        error: (err) => {
          this.loadingCountries.set(false);
          reject(err);
        },
      });
    });
  }

  private loadGovernoratesAsync(countryId: number): Promise<void> {
    this.loadingGovernorates.set(true);
    return new Promise((resolve) => {
      this.countriesApi.getGovernorates(countryId, false).subscribe({
        next: (items) => {
          this.governorates.set(items ?? []);
          this.loadingGovernorates.set(false);
          resolve();
        },
        error: (err) => {
          this.loadingGovernorates.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to load governorates.');
          resolve();
        },
      });
    });
  }

  private loadCitiesAsync(governorateId: number): Promise<void> {
    this.loadingCities.set(true);
    return new Promise((resolve) => {
      this.governoratesApi.getCities(governorateId, false).subscribe({
        next: (items) => {
          this.cities.set(items ?? []);
          this.loadingCities.set(false);
          resolve();
        },
        error: (err) => {
          this.loadingCities.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to load cities.');
          resolve();
        },
      });
    });
  }

  private loadAreasAsync(cityId: number): Promise<void> {
    this.loadingAreas.set(true);
    return new Promise((resolve) => {
      this.citiesApi.getAreas(cityId, false).subscribe({
        next: (items) => {
          this.areas.set(items ?? []);
          this.loadingAreas.set(false);
          resolve();
        },
        error: (err) => {
          this.loadingAreas.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to load areas.');
          resolve();
        },
      });
    });
  }
}
