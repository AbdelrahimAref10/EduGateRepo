import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/api/academy-api.generated';

export interface StudentExamListItem {
  examId: number;
  sessionId: number;
  lessonId: number;
  title: string;
  subject: string;
  groupName: string;
  topic?: string | null;
  teacherName: string;
  sessionDate: string;
  startTime: string;
  questionCount: number;
  sessionStarted: boolean;
  hasStarted: boolean;
  hasSubmitted: boolean;
  score?: number | null;
  maxScore?: number | null;
  percentage?: number | null;
  canTake: boolean;
}

@Injectable({ providedIn: 'root' })
export class StudentExamService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  getMyExams(): Observable<StudentExamListItem[]> {
    return this.http.get<StudentExamListItem[]>(`${this.baseUrl}/api/student/classroom/exams`);
  }
}
