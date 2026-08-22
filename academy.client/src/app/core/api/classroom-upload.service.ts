import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL, ClassroomMaterialDto, TeacherExamDto } from './academy-api.generated';

export interface ClassroomUploadParams {
  file: File;
  description?: string | null;
  title?: string | null;
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

  generateExam(
    sessionId: number,
    questionCount: number,
    minutesPerQuestion: number,
    files: File[],
  ): Observable<TeacherExamDto> {
    const form = new FormData();
    form.append('questionCount', String(questionCount));
    form.append('minutesPerQuestion', String(minutesPerQuestion));
    for (const file of files) {
      form.append('files', file, file.name);
    }

    const url = `${this.baseUrl}/api/teacher/classroom/sessions/${sessionId}/exam`;
    return this.http.post<unknown>(url, form).pipe(map((data) => TeacherExamDto.fromJS(data)));
  }
}
