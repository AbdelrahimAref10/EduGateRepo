import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  AdminUserListItemDto,
  AppRole,
  AreaDto,
  CitiesClient,
  CityDto,
  CountriesClient,
  CountryDto,
  CreateAdminUserRequest,
  GovernorateDto,
  GovernoratesClient,
  SetManageUsersPermissionRequest,
  UpdateAdminUserRoleRequest,
  UsersClient,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

interface RoleOption {
  value: AppRole;
  labelKey: string;
}

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, TranslatePipe],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsersComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usersApi = inject(UsersClient);
  private readonly countriesApi = inject(CountriesClient);
  private readonly governoratesApi = inject(GovernoratesClient);
  private readonly citiesApi = inject(CitiesClient);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly updatingRoleUserId = signal<number | null>(null);
  readonly updatingPermissionUserId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly users = signal<AdminUserListItemDto[]>([]);
  readonly countries = signal<CountryDto[]>([]);
  readonly governorates = signal<GovernorateDto[]>([]);
  readonly cities = signal<CityDto[]>([]);
  readonly areas = signal<AreaDto[]>([]);

  readonly loadingCountries = signal(false);
  readonly loadingGovernorates = signal(false);
  readonly loadingCities = signal(false);
  readonly loadingAreas = signal(false);

  readonly roles: RoleOption[] = [
    { value: AppRole.SuperAdmin, labelKey: 'auth.roleAdmin' },
    { value: AppRole.Teacher, labelKey: 'auth.roleTeacher' },
    { value: AppRole.Student, labelKey: 'auth.roleStudent' },
    { value: AppRole.Parent, labelKey: 'auth.roleParent' },
  ];

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],
    role: [AppRole.Teacher as AppRole, [Validators.required]],
    grantManageUsers: [false],
    countryId: [null as number | null],
    governorateId: [null as number | null],
    cityId: [null as number | null],
    areaId: [null as number | null],
  });

  readonly selectedRole = signal<AppRole>(AppRole.Teacher);
  readonly needsArea = computed(() => this.selectedRole() !== AppRole.SuperAdmin);
  readonly isCreatingSuperAdmin = computed(() => this.selectedRole() === AppRole.SuperAdmin);

  ngOnInit(): void {
    this.loadUsers();
    this.loadCountries();

    this.form.controls.role.valueChanges.subscribe((role) => {
      this.selectedRole.set(role);
      if (role === AppRole.SuperAdmin) {
        this.form.controls.countryId.setValue(null);
        this.form.controls.governorateId.setValue(null);
        this.form.controls.cityId.setValue(null);
        this.form.controls.areaId.setValue(null);
      }
    });

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

  loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);
    this.usersApi.getUsers().subscribe({
      next: (items) => {
        this.users.set(items ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.apiError(err, 'Failed to load users.'));
      },
    });
  }

  createUser(): void {
    this.error.set(null);
    this.success.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (value.password !== value.confirmPassword) {
      this.error.set('Password and confirm password do not match.');
      return;
    }

    if (value.role !== AppRole.SuperAdmin && !value.areaId) {
      this.error.set('Area is required for this role.');
      return;
    }

    const body = new CreateAdminUserRequest({
      email: value.email,
      password: value.password,
      confirmPassword: value.confirmPassword,
      firstName: value.firstName,
      lastName: value.lastName,
      phoneNumber: value.phoneNumber || undefined,
      role: value.role,
      areaId: value.role === AppRole.SuperAdmin ? undefined : (value.areaId ?? undefined),
      grantManageUsers: value.role === AppRole.SuperAdmin ? value.grantManageUsers : false,
    });

    this.saving.set(true);
    this.usersApi.createUser(body).subscribe({
      next: () => {
        this.saving.set(false);
        this.success.set('created');
        this.form.reset({
          firstName: '',
          lastName: '',
          email: '',
          phoneNumber: '',
          password: '',
          confirmPassword: '',
          role: AppRole.Teacher,
          grantManageUsers: false,
          countryId: null,
          governorateId: null,
          cityId: null,
          areaId: null,
        });
        this.selectedRole.set(AppRole.Teacher);
        this.loadUsers();
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(this.apiError(err, 'Failed to create user.'));
      },
    });
  }

  changeRole(user: AdminUserListItemDto, role: AppRole): void {
    if (this.primaryRole(user) === role) return;

    this.error.set(null);
    this.success.set(null);
    this.updatingRoleUserId.set(user.id);

    const body = new UpdateAdminUserRoleRequest({ role });
    this.usersApi.updateUserRole(user.id, body).subscribe({
      next: (updated) => {
        this.updatingRoleUserId.set(null);
        this.success.set('roleUpdated');
        this.users.update((list) =>
          list.map((item) => (item.id === updated.id ? updated : item)),
        );
      },
      error: (err) => {
        this.updatingRoleUserId.set(null);
        this.error.set(this.apiError(err, 'Failed to update role.'));
      },
    });
  }

  setManageUsers(user: AdminUserListItemDto, granted: boolean): void {
    if (!this.isSuperAdminUser(user) || !!user.hasManageUsers === granted) return;

    this.error.set(null);
    this.success.set(null);
    this.updatingPermissionUserId.set(user.id);

    this.usersApi
      .setManageUsersPermission(user.id, new SetManageUsersPermissionRequest({ granted }))
      .subscribe({
        next: (updated) => {
          this.updatingPermissionUserId.set(null);
          this.success.set('permissionUpdated');
          this.users.update((list) =>
            list.map((item) => (item.id === updated.id ? updated : item)),
          );
        },
        error: (err) => {
          this.updatingPermissionUserId.set(null);
          this.error.set(this.apiError(err, 'Failed to update permission.'));
        },
      });
  }

  isSuperAdminUser(user: AdminUserListItemDto): boolean {
    return (user.roles ?? []).includes('SuperAdmin');
  }

  primaryRole(user: AdminUserListItemDto): AppRole {
    const name = user.roles?.[0];
    switch (name) {
      case 'SuperAdmin':
        return AppRole.SuperAdmin;
      case 'Teacher':
        return AppRole.Teacher;
      case 'Parent':
        return AppRole.Parent;
      default:
        return AppRole.Student;
    }
  }

  roleLabelKey(roleName: string): string {
    switch (roleName) {
      case 'SuperAdmin':
        return 'auth.roleAdmin';
      case 'Teacher':
        return 'auth.roleTeacher';
      case 'Parent':
        return 'auth.roleParent';
      default:
        return 'auth.roleStudent';
    }
  }

  private loadCountries(): void {
    this.loadingCountries.set(true);
    this.countriesApi.getCountries(true).subscribe({
      next: (items) => {
        this.countries.set(items ?? []);
        this.loadingCountries.set(false);
      },
      error: () => this.loadingCountries.set(false),
    });
  }

  private loadGovernorates(countryId: number): void {
    this.loadingGovernorates.set(true);
    this.countriesApi.getGovernorates(countryId, true).subscribe({
      next: (items) => {
        this.governorates.set(items ?? []);
        this.loadingGovernorates.set(false);
      },
      error: () => this.loadingGovernorates.set(false),
    });
  }

  private loadCities(governorateId: number): void {
    this.loadingCities.set(true);
    this.governoratesApi.getCities(governorateId, true).subscribe({
      next: (items) => {
        this.cities.set(items ?? []);
        this.loadingCities.set(false);
      },
      error: () => this.loadingCities.set(false),
    });
  }

  private loadAreas(cityId: number): void {
    this.loadingAreas.set(true);
    this.citiesApi.getAreas(cityId, true).subscribe({
      next: (items) => {
        this.areas.set(items ?? []);
        this.loadingAreas.set(false);
      },
      error: () => this.loadingAreas.set(false),
    });
  }

  private apiError(err: unknown, fallback: string): string {
    const e = err as { detail?: string; title?: string; error?: { detail?: string; title?: string } };
    return e?.detail || e?.title || e?.error?.detail || e?.error?.title || fallback;
  }
}
