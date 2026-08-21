import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, from, map, of, switchMap, tap } from 'rxjs';
import {
  AccountClient,
  AppRole,
  UpdatePreferredLanguageRequest,
} from '../api/academy-api.generated';
import {
  AppRoleName,
  AuthResponse,
  AuthSession,
  ROLE_HOME,
} from './auth.models';
import { NotificationService } from '../notifications/notification.service';
import { TokenStorageService } from './token-storage.service';
import { TranslationService } from '../i18n/translation.service';
import { AppLanguage, languageIdFromCode } from '../i18n/i18n.models';

export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  role: AppRole;
  areaId: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly accountApi = inject(AccountClient);
  private readonly router = inject(Router);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly notifications = inject(NotificationService);
  private readonly i18n = inject(TranslationService);

  private readonly sessionSignal = signal<AuthSession | null>(this.tokenStorage.getSession());

  readonly session = this.sessionSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.sessionSignal()?.accessToken);
  readonly roles = computed(() => this.sessionSignal()?.roles ?? []);
  readonly fullName = computed(() => this.sessionSignal()?.fullName ?? '');
  readonly languageId = computed(() => this.sessionSignal()?.languageId ?? 1);
  readonly primaryRole = computed<AppRoleName | null>(() => this.roles()[0] ?? null);

  login(payload: LoginPayload): Observable<AuthSession> {
    return this.http.post<AuthResponse>('/api/auth/login', payload).pipe(
      switchMap((response) => from(this.persistAsync(response))),
      tap((session) => this.navigateByRole(session.roles)),
    );
  }

  register(payload: RegisterPayload): Observable<AuthSession> {
    return this.http.post<AuthResponse>('/api/auth/register', payload).pipe(
      switchMap((response) => from(this.persistAsync(response))),
      tap((session) => this.navigateByRole(session.roles)),
    );
  }

  refresh(): Observable<AuthSession | null> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      this.logout(false);
      return of(null);
    }

    return this.http.post<AuthResponse>('/api/auth/refresh', { refreshToken }).pipe(
      switchMap((response) => from(this.persistAsync(response))),
      catchError(() => {
        this.logout(false);
        return of(null);
      }),
    );
  }

  updatePreferredLanguage(code: AppLanguage): Observable<AuthSession> {
    const request = new UpdatePreferredLanguageRequest({
      languageId: languageIdFromCode(code),
    });

    return this.accountApi.updatePreferredLanguage(request).pipe(
      map((dto) => ({
        accessToken: dto.accessToken,
        refreshToken: dto.refreshToken,
        accessTokenExpiresAtUtc: dto.accessTokenExpiresAtUtc.toISOString(),
        refreshTokenExpiresAtUtc: dto.refreshTokenExpiresAtUtc.toISOString(),
        userId: dto.userId,
        email: dto.email,
        fullName: dto.fullName,
        roles: dto.roles ?? [],
        languageId: dto.languageId,
        studentCode: dto.studentCode,
        areaId: dto.areaId,
      }) satisfies AuthResponse),
      switchMap((response) => from(this.persistAsync(response))),
    );
  }

  logout(navigate = true): void {
    this.tokenStorage.clear();
    this.sessionSignal.set(null);
    this.notifications.clear();
    if (navigate) {
      void this.router.navigateByUrl('/login');
    }
  }

  patchSessionIdentity(patch: { email?: string; fullName?: string }): void {
    const next = this.tokenStorage.updateIdentity(patch);
    if (next) this.sessionSignal.set(next);
  }

  hasAnyRole(required: AppRoleName[]): boolean {
    const current = this.roles();
    return required.some((role) => current.includes(role));
  }

  homeForCurrentUser(): string {
    const role = this.primaryRole();
    return role ? ROLE_HOME[role] : '/login';
  }

  navigateByRole(roles: AppRoleName[]): void {
    const role = roles[0];
    void this.router.navigateByUrl(role ? ROLE_HOME[role] : '/login');
  }

  private async persistAsync(response: AuthResponse): Promise<AuthSession> {
    const session = this.tokenStorage.saveFromResponse(response);
    this.sessionSignal.set(session);
    await this.i18n.syncFromLanguageId(session.languageId);
    return session;
  }
}
