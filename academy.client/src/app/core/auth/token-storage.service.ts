import { Injectable } from '@angular/core';
import { AUTH_STORAGE_KEY, AuthResponse, AuthSession, AppRoleName } from './auth.models';
import { AppLanguageId } from '../i18n/i18n.models';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getSession(): AuthSession | null {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return null;

    try {
      const parsed = JSON.parse(raw) as AuthSession;
      return {
        ...parsed,
        permissions: parsed.permissions ?? [],
        languageId: parsed.languageId || AppLanguageId.Arabic,
      };
    } catch {
      localStorage.removeItem(AUTH_STORAGE_KEY);
      return null;
    }
  }

  saveFromResponse(response: AuthResponse): AuthSession {
    const session: AuthSession = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      accessTokenExpiresAtUtc: response.accessTokenExpiresAtUtc,
      refreshTokenExpiresAtUtc: response.refreshTokenExpiresAtUtc,
      userId: response.userId,
      email: response.email,
      fullName: response.fullName,
      roles: (response.roles ?? []).filter((role): role is AppRoleName =>
        ['SuperAdmin', 'Teacher', 'Student', 'Parent'].includes(role)),
      permissions: response.permissions ?? [],
      languageId: response.languageId || AppLanguageId.Arabic,
    };

    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
    return session;
  }

  updateIdentity(patch: { email?: string; fullName?: string }): AuthSession | null {
    const current = this.getSession();
    if (!current) return null;

    const next: AuthSession = {
      ...current,
      email: patch.email ?? current.email,
      fullName: patch.fullName ?? current.fullName,
    };

    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(next));
    return next;
  }

  clear(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
  }

  getAccessToken(): string | null {
    return this.getSession()?.accessToken ?? null;
  }

  getRefreshToken(): string | null {
    return this.getSession()?.refreshToken ?? null;
  }
}
