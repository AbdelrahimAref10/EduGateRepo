import { DatePipe } from '@angular/common';
import { Component, DestroyRef, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  StudentClient,
  TeacherStudentGroupDto,
  TeacherStudentLessonDto,
  TeacherStudentListItemDto,
  TransferStudentGroupRequest,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslationService } from '../../../core/i18n/translation.service';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import {
  billingKey,
  canSelectGroup,
  capacityLabel,
  capacityPercent,
  formatSchedule,
  groupStatusKey,
  parsePositiveId,
  readApiError,
} from './teacher-students.helpers';
import { TeacherStudentsNav } from './teacher-students-nav';

@Component({
  selector: 'app-teacher-student-group',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink, PageLoaderComponent],
  templateUrl: './teacher-student-group.html',
  styleUrl: './teacher-students.css',
})
export class TeacherStudentGroupComponent implements OnInit {
  private readonly api = inject(StudentClient);
  private readonly i18n = inject(TranslationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly nav = inject(TeacherStudentsNav);
  private readonly destroyRef = inject(DestroyRef);

  readonly studentId = signal<number | null>(null);
  readonly lessonId = signal<number | null>(null);
  readonly student = signal<TeacherStudentListItemDto | null>(null);
  readonly lesson = signal<TeacherStudentLessonDto | null>(null);
  readonly group = signal<TeacherStudentGroupDto | null>(null);
  readonly groupMissing = signal(false);
  readonly loading = signal(true);
  readonly loadingGroups = signal(false);
  readonly transferring = signal(false);
  readonly transferOpen = signal(false);
  readonly transferGroups = signal<TeacherStudentGroupDto[]>([]);
  readonly targetGroupId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal(false);

  readonly targetGroup = computed(() => {
    const id = this.targetGroupId();
    if (!id) return null;
    return this.transferGroups().find((item) => item.id === id) ?? null;
  });

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const studentId = parsePositiveId(params.get('studentId'));
      const lessonId = parsePositiveId(params.get('lessonId'));
      if (!studentId || !lessonId) {
        void this.router.navigate(['/teacher/students']);
        return;
      }
      this.studentId.set(studentId);
      this.lessonId.set(lessonId);
      this.closeTransfer();
      this.success.set(false);
      this.load(studentId, lessonId);
    });
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.transferOpen()) this.closeTransfer();
  }

  billingOf(type?: string): string {
    return billingKey(type);
  }

  scheduleOf(group: TeacherStudentGroupDto): string {
    return formatSchedule(group, this.i18n);
  }

  capacityOf(group: TeacherStudentGroupDto): string {
    return capacityLabel(group, this.i18n);
  }

  capacityPct(group: TeacherStudentGroupDto): number {
    return capacityPercent(group);
  }

  statusKey(group: TeacherStudentGroupDto): string {
    return groupStatusKey(group);
  }

  canPick(group: TeacherStudentGroupDto): boolean {
    return canSelectGroup(group);
  }

  openTransfer(): void {
    const studentId = this.studentId();
    const lessonId = this.lessonId();
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
        this.error.set(readApiError(err, 'Failed to load groups.'));
      },
    });
  }

  closeTransfer(): void {
    this.transferOpen.set(false);
    this.transferGroups.set([]);
    this.targetGroupId.set(null);
  }

  pickTarget(group: TeacherStudentGroupDto): void {
    if (!this.canPick(group) || this.transferring()) return;
    this.targetGroupId.set(group.id);
  }

  confirmTransfer(): void {
    const studentId = this.studentId();
    const lessonId = this.lessonId();
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
          this.success.set(true);
          this.nav.patchAssignedGroup(lessonId, target.id, target.name);
          this.lesson.update((item) => {
            if (!item) return item;
            item.assignedGroupId = target.id;
            item.assignedGroupName = target.name;
            return item;
          });
          this.loadGroup(studentId, lessonId, false);
        },
        error: (err) => {
          this.transferring.set(false);
          this.error.set(readApiError(err, 'Failed to move student.'));
        },
      });
  }

  private load(studentId: number, lessonId: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.group.set(null);
    this.groupMissing.set(false);

    const cachedStudent = this.nav.studentFor(studentId);
    const cachedLesson = this.nav.lessonFor(studentId, lessonId);
    this.student.set(cachedStudent);
    this.lesson.set(cachedLesson);

    const continueToGroup = (): void => this.loadGroup(studentId, lessonId, true);

    const loadLessons = (): void => {
      if (cachedLesson) {
        continueToGroup();
        return;
      }
      this.api.getStudentLessons(studentId).subscribe({
        next: (items) => {
          const list = items ?? [];
          this.nav.rememberLessons(studentId, list);
          this.lesson.set(list.find((item) => item.lessonId === lessonId) ?? null);
          continueToGroup();
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(readApiError(err, 'Failed to load lessons.'));
        },
      });
    };

    if (cachedStudent) {
      loadLessons();
      return;
    }

    this.api.getMyStudents(undefined).subscribe({
      next: (items) => {
        const found = (items ?? []).find((item) => item.studentId === studentId) ?? null;
        this.student.set(found);
        if (found) this.nav.rememberStudent(found);
        loadLessons();
      },
      error: (err) => {
        this.error.set(readApiError(err, 'Failed to load students.'));
        loadLessons();
      },
    });
  }

  private loadGroup(studentId: number, lessonId: number, keepLoading = true): void {
    if (keepLoading) this.loading.set(true);

    this.api.getStudentLessonGroup(studentId, lessonId).subscribe({
      next: (item) => {
        this.group.set(item);
        this.groupMissing.set(false);
        this.loading.set(false);
      },
      error: (err: { status?: number }) => {
        this.loading.set(false);
        if (err?.status === 404) {
          this.groupMissing.set(true);
          this.group.set(null);
          return;
        }
        this.error.set(readApiError(err, 'Failed to load group.'));
      },
    });
  }
}
