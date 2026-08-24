import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL, StudentExamDto } from './academy-api.generated';
import { DEFAULT_PAGE_SIZE, PagedResult, mapPagedResult } from './paging';

export interface ParentChildDto {
  childStudentId: number;
  fullName: string;
  studentCode?: string | null;
  photoUrl?: string | null;
  linkedAtUtc: string;
}

export interface ParentUpcomingSessionDto {
  sessionId: number;
  childStudentId: number;
  childName: string;
  subject: string;
  groupName: string;
  teacherName: string;
  sessionDate: string;
  startTime: string;
  topic?: string | null;
  hasStarted: boolean;
}

export interface ParentUnpaidChargeDto {
  chargeId: number;
  childStudentId: number;
  childName: string;
  subject: string;
  type: string;
  amount: number;
  remaining: number;
  status: string;
  createdAtUtc: string;
}

export interface ParentDashboardDto {
  children: ParentChildDto[];
  upcomingSessions: ParentUpcomingSessionDto[];
  unpaidCharges: ParentUnpaidChargeDto[];
}

export interface ParentExamListItemDto {
  examId: number;
  sessionId: number;
  childStudentId: number;
  childName: string;
  title: string;
  subject: string;
  groupName: string;
  teacherName: string;
  sessionDate: string;
  startTime: string;
  questionCount: number;
  hasSubmitted: boolean;
  score?: number | null;
  maxScore?: number | null;
  percentage?: number | null;
}

export interface ParentAttendanceItemDto {
  sessionId: number;
  childStudentId: number;
  childName: string;
  subject: string;
  groupName: string;
  sessionDate: string;
  startTime: string;
  isPresent: boolean;
  teacherNotes?: string | null;
}

export interface ParentPaymentItemDto {
  paymentId: number;
  childStudentId: number;
  childName: string;
  subject: string;
  amount: number;
  method: string;
  receiptNumber: number;
  paidAtUtc: string;
  note?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ParentApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  getDashboard(): Observable<ParentDashboardDto> {
    return this.http.get<ParentDashboardDto>(`${this.baseUrl}/api/parent/dashboard`);
  }

  getChildren(): Observable<ParentChildDto[]> {
    return this.http.get<ParentChildDto[]>(`${this.baseUrl}/api/parent/children`);
  }

  linkChild(studentCode: string): Observable<ParentChildDto> {
    return this.http.post<ParentChildDto>(`${this.baseUrl}/api/parent/children/link`, {
      studentCode,
    });
  }

  unlinkChild(childStudentId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/parent/children/${childStudentId}`);
  }

  getExams(
    childStudentId?: number | null,
    page = 1,
    pageSize = DEFAULT_PAGE_SIZE,
  ): Observable<PagedResult<ParentExamListItemDto>> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));
    if (childStudentId) params = params.set('childStudentId', String(childStudentId));
    return this.http
      .get<unknown>(`${this.baseUrl}/api/parent/exams`, { params })
      .pipe(map((data) => mapPagedResult(data, (item) => item as ParentExamListItemDto)));
  }

  getChildExam(childStudentId: number, sessionId: number): Observable<StudentExamDto | null> {
    return this.http.get<StudentExamDto | null>(
      `${this.baseUrl}/api/parent/children/${childStudentId}/sessions/${sessionId}/exam`,
    );
  }

  getAttendance(
    childStudentId?: number | null,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<ParentAttendanceItemDto>> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));
    if (childStudentId) params = params.set('childStudentId', String(childStudentId));
    return this.http
      .get<unknown>(`${this.baseUrl}/api/parent/attendance`, { params })
      .pipe(map((data) => mapPagedResult(data, (item) => item as ParentAttendanceItemDto)));
  }

  getPayments(
    childStudentId?: number | null,
    page = 1,
    pageSize = 20,
  ): Observable<PagedResult<ParentPaymentItemDto>> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));
    if (childStudentId) params = params.set('childStudentId', String(childStudentId));
    return this.http
      .get<unknown>(`${this.baseUrl}/api/parent/payments`, { params })
      .pipe(map((data) => mapPagedResult(data, (item) => item as ParentPaymentItemDto)));
  }
}
