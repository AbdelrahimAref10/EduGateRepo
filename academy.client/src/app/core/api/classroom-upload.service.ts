import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL, ClassroomMaterialDto, TeacherExamDto } from './academy-api.generated';

export interface ClassroomUploadParams {
  file: File;
  description?: string | null;
  title?: string | null;
}

export interface TeacherExamReviewOption {
  id: number;
  text: string;
  isCorrect: boolean;
  sortOrder?: number;
}

export interface TeacherExamReviewQuestion {
  id: number;
  text: string;
  sortOrder?: number;
  selectedOptionId?: number | null;
  options?: TeacherExamReviewOption[] | null;
}

export interface TeacherStudentExamReview {
  studentId: number;
  studentName: string;
  studentCode?: string | null;
  title?: string;
  hasSubmitted: boolean;
  score?: number | null;
  maxScore?: number | null;
  percentage?: number | null;
  submittedAtUtc?: string | null;
  questions?: TeacherExamReviewQuestion[] | null;
}

export interface TeacherExamResults {
  examId: number;
  title: string;
  status: number;
  submittedCount: number;
  studentCount: number;
  students: TeacherStudentExamReview[];
}

@Injectable({ providedIn: 'root' })
export class ClassroomUploadService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  uploadMaterial(sessionId: number, params: ClassroomUploadParams): Observable<ClassroomMaterialDto> {
    const form = new FormData();
    form.append('file', params.file, params.file.name);
    if (params.description?.trim()) {
      form.append('description', params.description.trim());
    }
    if (params.title?.trim()) {
      form.append('title', params.title.trim());
    }

    const url = `${this.baseUrl}/api/teacher/classroom/sessions/${sessionId}/materials/upload`;
    return this.http
      .post<unknown>(url, form)
      .pipe(map((data) => ClassroomMaterialDto.fromJS(data)));
  }

  generateExam(sessionId: number, questionCount: number, files: File[]): Observable<TeacherExamDto> {
    const form = new FormData();
    form.append('questionCount', String(questionCount));
    for (const file of files) {
      form.append('files', file, file.name);
    }

    const url = `${this.baseUrl}/api/teacher/classroom/sessions/${sessionId}/exam`;
    return this.http.post<unknown>(url, form).pipe(map((data) => TeacherExamDto.fromJS(data)));
  }

  getExamResults(sessionId: number): Observable<TeacherExamResults> {
    const url = `${this.baseUrl}/api/teacher/classroom/sessions/${sessionId}/exam/results`;
    return this.http.get<TeacherExamResults>(url);
  }

  getStudentExamReview(sessionId: number, studentId: number): Observable<TeacherStudentExamReview> {
    const url = `${this.baseUrl}/api/teacher/classroom/sessions/${sessionId}/exam/results/${studentId}`;
    return this.http.get<TeacherStudentExamReview>(url);
  }
}

