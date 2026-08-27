import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { StudentClient, TeacherStudentListItemDto } from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslationService } from '../../../core/i18n/translation.service';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { copyText, parentsLabel, readApiError } from './teacher-students.helpers';
import { TeacherStudentsNav } from './teacher-students-nav';

@Component({
  selector: 'app-teacher-students',
  standalone: true,
  imports: [TranslatePipe, RouterLink, PageLoaderComponent, UserAvatarComponent],
  templateUrl: './teacher-students.html',
  styleUrl: './teacher-students.css',
})
export class TeacherStudentsComponent implements OnInit {
  private readonly api = inject(StudentClient);
  private readonly i18n = inject(TranslationService);
  private readonly nav = inject(TeacherStudentsNav);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly copied = signal(false);
  readonly searchDraft = signal('');
  readonly students = signal<TeacherStudentListItemDto[]>([]);

  readonly studentsCount = computed(() => this.students().length);
  readonly withParentsCount = computed(
    () => this.students().filter((item) => (item.parents?.length ?? 0) > 0).length,
  );
  readonly lessonsTotal = computed(() =>
    this.students().reduce((sum, item) => sum + (item.lessonsCount ?? 0), 0),
  );
  readonly searching = computed(() => this.searchDraft().trim().length > 0);

  ngOnInit(): void {
    this.search$
      .pipe(debounceTime(320), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((term) => this.loadStudents(term));

    this.loadStudents('');
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchDraft.set(value);
    this.search$.next(value.trim());
  }

  refresh(): void {
    this.loadStudents(this.searchDraft().trim());
  }

  remember(student: TeacherStudentListItemDto): void {
    this.nav.rememberStudent(student);
  }

  async copyCode(event: Event, code?: string | null): Promise<void> {
    event.stopPropagation();
    if (!(await copyText(code))) return;
    this.copied.set(true);
    window.setTimeout(() => this.copied.set(false), 1800);
  }

  parentsOf(student: TeacherStudentListItemDto): string {
    return parentsLabel(student, this.i18n);
  }

  private loadStudents(search: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getMyStudents(search || undefined).subscribe({
      next: (items) => {
        this.students.set(items ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(readApiError(err, 'Failed to load students.'));
      },
    });
  }
}
