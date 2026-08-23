import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL, StudentExamListItemDto } from './academy-api.generated';
import { DEFAULT_PAGE_SIZE, PagedResult, mapPagedResult } from './paging';

@Injectable({ providedIn: 'root' })
export class StudentExamsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  getMyExams(page = 1, pageSize = DEFAULT_PAGE_SIZE): Observable<PagedResult<StudentExamListItemDto>> {
    const params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));

    return this.http
      .get<unknown>(`${this.baseUrl}/api/student/classroom/exams`, { params })
      .pipe(map((data) => mapPagedResult(data, (item) => StudentExamListItemDto.fromJS(item))));
  }
}
