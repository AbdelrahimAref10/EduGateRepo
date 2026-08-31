import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './academy-api.generated';

export interface WeeklyPlanSessionDto {
  sessionId: number;
  lessonId: number;
  studentId: number;
  studentName: string;
  subject: string;
  groupName: string;
  teacherName: string;
  sessionDate: string;
  startTime: string;
  topic?: string | null;
  notes?: string | null;
  hasStarted: boolean;
  isPresent?: boolean | null;
}

export interface WeeklyPlanChargeDto {
  chargeId: number;
  studentId: number;
  studentName: string;
  lessonId: number;
  subject: string;
  type: string;
  remaining: number;
}

export interface WeeklyPlanExamDto {
  examId: number;
  sessionId: number;
  studentId: number;
  studentName: string;
  title: string;
  subject: string;
  sessionDate: string;
  sessionStarted: boolean;
  hasSubmitted: boolean;
}

export interface WeeklyLearningPlanDto {
  weekStart: string;
  weekEnd: string;
  sessions: WeeklyPlanSessionDto[];
  unpaidCharges: WeeklyPlanChargeDto[];
  examsDue: WeeklyPlanExamDto[];
}

export interface RecentSessionProgressDto {
  sessionId: number;
  sessionDate: string;
  startTime: string;
  topic?: string | null;
  teacherNotes?: string | null;
  hasStarted: boolean;
  isPresent?: boolean | null;
}

export interface LessonProgressDto {
  studentId: number;
  studentName: string;
  lessonId: number;
  groupId?: number | null;
  subject: string;
  groupName: string;
  teacherName: string;
  sessionsHeld: number;
  sessionsPresent: number;
  attendancePercent?: number | null;
  examsTaken: number;
  examAveragePercent?: number | null;
  outstanding: number;
  recentSessions: RecentSessionProgressDto[];
}

export interface ProgressReportDto {
  lessons: LessonProgressDto[];
}

export interface TeacherGroupProgressDto {
  lessonId: number;
  groupId: number;
  subject: string;
  groupName: string;
  members: LessonProgressDto[];
}

@Injectable({ providedIn: 'root' })
export class LearningPathApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  getStudentPlan(): Observable<WeeklyLearningPlanDto> {
    return this.http.get<WeeklyLearningPlanDto>(`${this.baseUrl}/api/student/learning/plan`);
  }

  getStudentProgress(): Observable<ProgressReportDto> {
    return this.http.get<ProgressReportDto>(`${this.baseUrl}/api/student/learning/progress`);
  }

  getParentPlan(childStudentId?: number | null): Observable<WeeklyLearningPlanDto> {
    let params = new HttpParams();
    if (childStudentId) params = params.set('childStudentId', String(childStudentId));
    return this.http.get<WeeklyLearningPlanDto>(`${this.baseUrl}/api/parent/learning/plan`, { params });
  }

  getParentProgress(childStudentId?: number | null): Observable<ProgressReportDto> {
    let params = new HttpParams();
    if (childStudentId) params = params.set('childStudentId', String(childStudentId));
    return this.http.get<ProgressReportDto>(`${this.baseUrl}/api/parent/learning/progress`, { params });
  }

  getTeacherStudentProgress(studentId: number): Observable<ProgressReportDto> {
    return this.http.get<ProgressReportDto>(`${this.baseUrl}/api/teacher/student/${studentId}/progress`);
  }

  getTeacherGroupProgress(lessonId: number, groupId: number): Observable<TeacherGroupProgressDto> {
    return this.http.get<TeacherGroupProgressDto>(
      `${this.baseUrl}/api/teacher/lessons/${lessonId}/groups/${groupId}/progress`,
    );
  }
}
