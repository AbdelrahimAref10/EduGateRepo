import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  StudentClient,
  TeacherStudentLessonDto,
  TeacherStudentListItemDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { billingKey, copyText, parsePositiveId, readApiError } from './teacher-students.helpers';
import { TeacherStudentsNav } from './teacher-students-nav';

@Component({
  selector: 'app-teacher-student-lessons',
  standalone: true,
  imports: [TranslatePipe, RouterLink, PageLoaderComponent, UserAvatarComponent],
  templateUrl: './teacher-student-lessons.html',
  styleUrl: './teacher-students.css',
})
export class TeacherStudentLessonsComponent implements OnInit {
  private readonly api = inject(StudentClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly nav = inject(TeacherStudentsNav);
  private readonly destroyRef = inject(DestroyRef);

  readonly studentId = signal<number | null>(null);
  readonly student = signal<TeacherStudentListItemDto | null>(null);
  readonly lessons = signal<TeacherStudentLessonDto[]>([]);
  readonly loading = signal(true);
  readonly loadingStudent = signal(false);
  readonly error = signal<string | null>(null);
  readonly copied = signal(false);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const id = parsePositiveId(params.get('studentId'));
      if (!id) {
        void this.router.navigate(['/teacher/students']);
        return;
      }
      this.studentId.set(id);
      const legacyLessonId = parsePositiveId(this.route.snapshot.queryParamMap.get('lesson'));
      if (legacyLessonId) {
        void this.router.navigate(['/teacher/students', id, 'lessons', legacyLessonId], { replaceUrl: true });
        return;
      }
      this.bootstrap(id);
    });
  }

  billingOf(type?: string): string {
    return billingKey(type);
  }

  async copyCode(code?: string | null): Promise<void> {
    if (!(await copyText(code))) return;
    this.copied.set(true);
    window.setTimeout(() => this.copied.set(false), 1800);
  }

  rememberLessons(): void {
    const studentId = this.studentId();
    if (!studentId) return;
    this.nav.rememberLessons(studentId, this.lessons());
  }

  private bootstrap(studentId: number): void {
    const cachedStudent = this.nav.studentFor(studentId);
    this.student.set(cachedStudent);
    this.loadingStudent.set(!cachedStudent);
    this.loading.set(true);
    this.error.set(null);
    this.lessons.set(this.nav.lessonsFor(studentId) ?? []);

    if (!cachedStudent) {
      this.api.getMyStudents(undefined).subscribe({
        next: (items) => {
          const found = (items ?? []).find((item) => item.studentId === studentId) ?? null;
          this.student.set(found);
          this.loadingStudent.set(false);
          if (found) this.nav.rememberStudent(found);
        },
        error: (err) => {
          this.loadingStudent.set(false);
          this.error.set(readApiError(err, 'Failed to load students.'));
        },
      });
    }

    this.api.getStudentLessons(studentId).subscribe({
      next: (items) => {
        const list = items ?? [];
        this.lessons.set(list);
        this.nav.rememberLessons(studentId, list);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(readApiError(err, 'Failed to load lessons.'));
      },
    });
  }
}
