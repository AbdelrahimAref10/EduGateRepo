import { DatePipe } from '@angular/common';
import { Component, HostListener, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
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
import { TranslationService } from '../../../core/i18n/translation.service';

interface RoleOption {
  value: AppRole;
  name: 'SuperAdmin' | 'Teacher' | 'Student' | 'Parent';
  labelKey: string;
}

type RoleFilter = 'all' | RoleOption['name'];

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, TranslatePipe, DatePipe],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsersComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly i18n = inject(TranslationService);
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
    { value: AppRole.SuperAdmin, name: 'SuperAdmin', labelKey: 'auth.roleAdmin' },
    { value: AppRole.Teacher, name: 'Teacher', labelKey: 'auth.roleTeacher' },
    { value: AppRole.Student, name: 'Student', labelKey: 'auth.roleStudent' },
    { value: AppRole.Parent, name: 'Parent', labelKey: 'auth.roleParent' },
  ];

  readonly filters: { id: RoleFilter; labelKey: string }[] = [
    { id: 'all', labelKey: 'adminUsers.statAll' },
    { id: 'SuperAdmin', labelKey: 'auth.roleAdmin' },
    { id: 'Teacher', labelKey: 'auth.roleTeacher' },
    { id: 'Student', labelKey: 'auth.roleStudent' },
    { id: 'Parent', labelKey: 'auth.roleParent' },
  ];

  readonly search = signal('');
  readonly roleFilter = signal<RoleFilter>('all');
  readonly createOpen = signal(false);

  readonly counts = computed(() => {
    const list = this.users();
    const has = (name: RoleOption['name']) => list.filter((user) => (user.roles ?? []).includes(name)).length;
    return {
      all: list.length,
      SuperAdmin: has('SuperAdmin'),
      Teacher: has('Teacher'),
      Student: has('Student'),
      Parent: has('Parent'),
    };
  });

  readonly filteredUsers = computed(() => {
    const query = this.search().trim().toLowerCase();
    const filter = this.roleFilter();
    return this.users().filter((user) => {
      if (filter !== 'all' && !(user.roles ?? []).includes(filter)) return false;
      if (!query) return true;
      const hay = [user.fullName, user.email, user.phoneNumber, user.studentCode]
        .filter(Boolean)
        .join(' ')
        .toLowerCase();
      return hay.includes(query);
    });
  });

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

  ngOnDestroy(): void {
    document.body.style.overflow = '';
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
      this.error.set(this.i18n.t('adminUsers.passwordMismatch'));
      return;
    }

    if (value.role !== AppRole.SuperAdmin && !value.areaId) {
      this.error.set(this.i18n.t('adminUsers.areaRequired'));
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
        this.createOpen.set(false);
        document.body.style.overflow = '';
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

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.createOpen() && !this.saving()) this.closeCreate();
  }

  openCreate(): void {
    this.error.set(null);
    this.createOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeCreate(): void {
    if (this.saving()) return;
    this.createOpen.set(false);
    document.body.style.overflow = '';
  }

  countFor(id: RoleFilter): number {
    return this.counts()[id];
  }

  setCreateRole(role: AppRole): void {
    this.form.controls.role.setValue(role);
  }

  setRoleFilter(filter: RoleFilter): void {
    this.roleFilter.set(filter);
  }

  onSearch(value: string): void {
    this.search.set(value);
  }

  initials(name?: string): string {
    const parts = (name ?? '').trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  roleTone(roleName?: string): string {
    switch (roleName) {
      case 'SuperAdmin':
        return 'admin';
      case 'Teacher':
        return 'teacher';
      case 'Parent':
        return 'parent';
      default:
        return 'student';
    }
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
