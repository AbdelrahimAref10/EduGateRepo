export type AppRoleName = 'SuperAdmin' | 'Teacher' | 'Student' | 'Parent';

export const MANAGE_USERS_PERMISSION = 'ManageUsers';

export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  refreshTokenExpiresAtUtc: string;
  userId: number;
  email: string;
  fullName: string;
  roles: AppRoleName[];
  permissions: string[];
  languageId: number;
  photoUrl?: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
  refreshTokenExpiresAtUtc: string;
  userId: number;
  email: string;
  fullName: string;
  roles: string[];
  permissions?: string[];
  languageId: number;
  studentCode?: string | null;
  areaId?: number | null;
  photoUrl?: string | null;
}

export const AUTH_STORAGE_KEY = 'academy.auth.session';

export const ROLE_HOME: Record<AppRoleName, string> = {
  SuperAdmin: '/super-admin',
  Teacher: '/teacher',
  Student: '/student',
  Parent: '/parent',
};
