import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  HostListener,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, combineLatest, debounceTime, distinctUntilChanged } from 'rxjs';
import {
  DayOfWeek,
  StudentClient,
  TeacherStudentGroupDto,
  TeacherStudentLessonDto,
  TeacherStudentListItemDto,
  TransferStudentGroupRequest,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslationService } from '../../../core/i18n/translation.service';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

@Component({
  selector: 'app-teacher-students',
  standalone: true,
  imports: [TranslatePipe, DatePipe, PageLoaderComponent, UserAvatarComponent],
  templateUrl: './teacher-students.html',
  styleUrl: './teacher-students.css',
})
export class TeacherStudentsComponent implements OnInit {
  private readonly api = inject(StudentClient);
  private readonly i18n = inject(TranslationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly search$ = new Subject<string>();

  readonly loading = signal(true);
  readonly loadingLessons = signal(false);
  readonly loadingGroup = signal(false);
  readonly loadingGroups = signal(false);
  readonly transferring = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly copied = signal(false);
  readonly searchDraft = signal('');

  readonly students = signal<TeacherStudentListItemDto[]>([]);
  readonly cachedStudent = signal<TeacherStudentListItemDto | null>(null);
  readonly selectedStudentId = signal<number | null>(null);
  readonly lessons = signal<TeacherStudentLessonDto[]>([]);
  readonly selectedLessonId = signal<number | null>(null);
  readonly group = signal<TeacherStudentGroupDto | null>(null);
  readonly groupMissing = signal(false);
  readonly transferOpen = signal(false);
  readonly transferGroups = signal<TeacherStudentGroupDto[]>([]);
  readonly targetGroupId = signal<number | null>(null);

  readonly selectedStudent = computed(() => {
    const id = this.selectedStudentId();
    if (!id) return null;
    return this.students().find((item) => item.studentId === id) ?? this.cachedStudent();
  });

  readonly selectedLesson = computed(() => {
    const id = this.selectedLessonId();
    if (!id) return null;
    return this.lessons().find((item) => item.lessonId === id) ?? null;
  });

  readonly targetGroup = computed(() => {
    const id = this.targetGroupId();
    if (!id) return null;
    return this.transferGroups().find((item) => item.id === id) ?? null;
  });

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

    combineLatest([this.route.paramMap, this.route.queryParamMap])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([params, query]) => {
        const studentId = this.parseId(params.get('studentId'));
        const lessonId = this.parseId(query.get('lesson'));
        const studentChanged = this.selectedStudentId() !== studentId;
        const lessonChanged = this.selectedLessonId() !== lessonId;

        this.selectedStudentId.set(studentId);
        this.selectedLessonId.set(lessonId);

        if (studentChanged) {
          this.success.set(null);
          this.closeTransfer();
          if (!studentId) this.cachedStudent.set(null);
          if (studentId) this.loadLessons(studentId);
          else {
            this.lessons.set([]);
            this.group.set(null);
            this.groupMissing.set(false);
          }
          return;
        }

        if (lessonChanged) {
          this.closeTransfer();
          if (studentId && lessonId) this.loadGroup(studentId, lessonId);
          else {
            this.group.set(null);
            this.groupMissing.set(false);
          }
        }
      });

    this.loadStudents('');
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.transferOpen()) this.closeTransfer();
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchDraft.set(value);
    this.search$.next(value.trim());
  }

  refresh(): void {
    this.loadStudents(this.searchDraft().trim());
  }

  openStudent(student: TeacherStudentListItemDto): void {
    this.cachedStudent.set(student);
    void this.router.navigate(['/teacher/students', student.studentId]);
  }

  backToList(): void {
    void this.router.navigate(['/teacher/students']);
  }

  openLesson(lesson: TeacherStudentLessonDto): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { lesson: lesson.lessonId },
    });
  }

  async copyCode(code?: string | null): Promise<void> {
    if (!code) return;
    try {
      await navigator.clipboard.writeText(code);
      this.copied.set(true);
      window.setTimeout(() => this.copied.set(false), 1800);
    } catch {
      this.copied.set(false);
    }
  }

  parentsLabel(student: TeacherStudentListItemDto): string {
    const parents = student.parents ?? [];
    if (parents.length === 0) return this.i18n.t('myStudents.noParent');
    return parents
      .map((parent) => (parent.phoneNumber ? `${parent.fullName} · ${parent.phoneNumber}` : parent.fullName))
      .join(' · ');
  }

  billingKey(type?: string): string {
    return type === 'Monthly' ? 'lessons.monthly' : 'lessons.perSession';
  }

  dayLabel(day: DayOfWeek | number): string {
    switch (Number(day)) {
      case DayOfWeek.Sunday:
        return this.i18n.t('lessons.daySunday');
      case DayOfWeek.Monday:
        return this.i18n.t('lessons.dayMonday');
      case DayOfWeek.Tuesday:
        return this.i18n.t('lessons.dayTuesday');
      case DayOfWeek.Wednesday:
        return this.i18n.t('lessons.dayWednesday');
      case DayOfWeek.Thursday:
        return this.i18n.t('lessons.dayThursday');
      case DayOfWeek.Friday:
        return this.i18n.t('lessons.dayFriday');
      case DayOfWeek.Saturday:
        return this.i18n.t('lessons.daySaturday');
      default:
        return String(day);
    }
  }

  formatTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  formatSchedule(group: TeacherStudentGroupDto): string {
    const dates = group.dates ?? [];
    if (dates.length === 0) return '—';
    return dates.map((d) => `${this.dayLabel(d.dayOfWeek)} ${this.formatTime(d.startTime)}`).join(' · ');
  }

  capacityLabel(group: TeacherStudentGroupDto): string {
    if (group.maxCapacity == null) return this.i18n.t('myStudents.unlimited');
    return `${group.membersCount} ${this.i18n.t('myStudents.seatsOf')} ${group.maxCapacity}`;
  }

  capacityPercent(group: TeacherStudentGroupDto): number {
    if (!group.maxCapacity || group.maxCapacity <= 0) return 12;
    return Math.min(100, Math.round((group.membersCount / group.maxCapacity) * 100));
  }

  groupStatusKey(group: TeacherStudentGroupDto): string {
    if (group.hasEnded) return 'myStudents.ended';
    if (group.isFull) return 'myStudents.full';
    if (group.isEmpty) return 'myStudents.emptyGroup';
    if (group.hasStarted) return 'myStudents.running';
    return 'myStudents.openSeats';
  }

  canSelectGroup(group: TeacherStudentGroupDto): boolean {
    return !group.isCurrentStudentGroup && !group.isFull && !group.hasEnded;
  }

  openTransfer(): void {
    const studentId = this.selectedStudentId();
    const lessonId = this.selectedLessonId();
    if (!studentId || !lessonId || !this.group()) return;

    this.transferOpen.set(true);
    this.targetGroupId.set(null);
    this.loadingGroups.set(true);
    this.error.set(null);

    this.api.getLessonGroupsForTransfer(studentId, lessonId).subscribe({
      next: (items) => {
        this.transferGroups.set(items ?? []);
        this.loadingGroups.set(false);
      },
      error: (err) => {
        this.loadingGroups.set(false);
        this.error.set(this.readError(err, 'Failed to load groups.'));
      },
    });
  }

  closeTransfer(): void {
    this.transferOpen.set(false);
    this.transferGroups.set([]);
    this.targetGroupId.set(null);
  }

  pickTarget(group: TeacherStudentGroupDto): void {
    if (!this.canSelectGroup(group) || this.transferring()) return;
    this.targetGroupId.set(group.id);
  }

  confirmTransfer(): void {
    const studentId = this.selectedStudentId();
    const lessonId = this.selectedLessonId();
    const target = this.targetGroup();
    if (!studentId || !lessonId || !target || this.transferring()) return;

    this.transferring.set(true);
    this.error.set(null);

    this.api
      .transferStudentGroup(studentId, lessonId, new TransferStudentGroupRequest({ targetGroupId: target.id }))
      .subscribe({
        next: () => {
          this.transferring.set(false);
          this.closeTransfer();
          this.success.set('transferred');
          this.loadGroup(studentId, lessonId);
          this.lessons.update((items) =>
            items.map((item) => {
              if (item.lessonId !== lessonId) return item;
              item.assignedGroupId = target.id;
              item.assignedGroupName = target.name;
              return item;
            }),
          );
        },
        error: (err) => {
          this.transferring.set(false);
          this.error.set(this.readError(err, 'Failed to move student.'));
        },
      });
  }

  private loadStudents(search: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getMyStudents(search || undefined).subscribe({
      next: (items) => {
        this.students.set(items ?? []);
        this.loading.set(false);
        const selectedId = this.selectedStudentId();
        const found = selectedId
          ? (items ?? []).find((item) => item.studentId === selectedId)
          : null;
        if (found) this.cachedStudent.set(found);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(this.readError(err, 'Failed to load students.'));
      },
    });
  }

  private loadLessons(studentId: number): void {
    this.loadingLessons.set(true);
    this.lessons.set([]);

    this.api.getStudentLessons(studentId).subscribe({
      next: (items) => {
        this.lessons.set(items ?? []);
        this.loadingLessons.set(false);
        const lessonId = this.selectedLessonId();
        if (lessonId) this.loadGroup(studentId, lessonId);
      },
      error: (err) => {
        this.loadingLessons.set(false);
        this.error.set(this.readError(err, 'Failed to load lessons.'));
      },
    });
  }

  private loadGroup(studentId: number, lessonId: number): void {
    this.loadingGroup.set(true);
    this.group.set(null);
    this.groupMissing.set(false);

    this.api.getStudentLessonGroup(studentId, lessonId).subscribe({
      next: (item) => {
        this.group.set(item);
        this.groupMissing.set(false);
        this.loadingGroup.set(false);
      },
      error: (err: { status?: number }) => {
        this.loadingGroup.set(false);
        if (err?.status === 404) {
          this.groupMissing.set(true);
          this.group.set(null);
          return;
        }
        this.error.set(this.readError(err, 'Failed to load group.'));
      },
    });
  }

  private parseId(value: string | null): number | null {
    if (!value) return null;
    const id = Number(value);
    return Number.isInteger(id) && id > 0 ? id : null;
  }

  private readError(err: unknown, fallback: string): string {
    const error = err as { status?: number; result?: { detail?: string }; message?: string };
    return error?.result?.detail || error?.message || fallback;
  }
}
