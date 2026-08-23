import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
  API_BASE_URL,
  LedgerStudentRowDto,
  StudentLessonLedgerDto,
} from './academy-api.generated';

export interface AdminDebtRowDto {
  lessonId: number;
  subject: string;
  teacherName: string;
  studentId: number;
  studentName: string;
  studentCode?: string;
  photoUrl?: string;
  outstandingAmount: number;
  openChargesCount: number;
}

export interface AdminFileResponse {
  data: Blob;
  fileName?: string;
}

function parseFileName(contentDisposition: string | null): string | undefined {
  if (!contentDisposition) return undefined;
  let match = /filename\*=(?:(\\?['"])(.*?)\1|(?:[^\s]+'.*?')?([^;\n]*))/g.exec(contentDisposition);
  let fileName = match && match.length > 1 ? match[3] || match[2] : undefined;
  if (fileName) return decodeURIComponent(fileName);
  match = /filename="?([^"]*?)"?(;|$)/g.exec(contentDisposition);
  return match && match.length > 1 ? match[1] : undefined;
}

function debtFromJs(data: unknown): AdminDebtRowDto {
  const d = (typeof data === 'object' && data ? data : {}) as Record<string, unknown>;
  return {
    lessonId: Number(d['lessonId'] ?? 0),
    subject: String(d['subject'] ?? ''),
    teacherName: String(d['teacherName'] ?? ''),
    studentId: Number(d['studentId'] ?? 0),
    studentName: String(d['studentName'] ?? ''),
    studentCode: d['studentCode'] != null ? String(d['studentCode']) : undefined,
    photoUrl: d['photoUrl'] != null ? String(d['photoUrl']) : undefined,
    outstandingAmount: Number(d['outstandingAmount'] ?? 0),
    openChargesCount: Number(d['openChargesCount'] ?? 0),
  };
}

@Injectable({ providedIn: 'root' })
export class AdminBillingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  getDebts(lessonId?: number | null): Observable<AdminDebtRowDto[]> {
    const params =
      lessonId != null && lessonId > 0 ? `?lessonId=${encodeURIComponent(String(lessonId))}` : '';
    const url = `${this.baseUrl}/api/super-admin/billing/debts${params}`;
    return this.http.get<unknown[]>(url).pipe(map((rows) => (rows ?? []).map(debtFromJs)));
  }

  getGroupLedger(lessonId: number, groupId: number): Observable<LedgerStudentRowDto[]> {
    const url = `${this.baseUrl}/api/super-admin/billing/lessons/${lessonId}/groups/${groupId}/ledger`;
    return this.http
      .get<unknown[]>(url)
      .pipe(map((rows) => (rows ?? []).map((item) => LedgerStudentRowDto.fromJS(item))));
  }

  getStudentLedger(lessonId: number, studentId: number): Observable<StudentLessonLedgerDto> {
    const url = `${this.baseUrl}/api/super-admin/billing/lessons/${lessonId}/students/${studentId}/ledger`;
    return this.http.get<unknown>(url).pipe(map((data) => StudentLessonLedgerDto.fromJS(data)));
  }

  downloadReceipt(paymentId: number): Observable<AdminFileResponse> {
    const url = `${this.baseUrl}/api/super-admin/billing/payments/${paymentId}/receipt`;
    return this.http.get(url, { observe: 'response', responseType: 'blob' }).pipe(
      map((response: HttpResponse<Blob>) => ({
        data: response.body as Blob,
        fileName: parseFileName(response.headers.get('content-disposition')),
      })),
    );
  }
}
