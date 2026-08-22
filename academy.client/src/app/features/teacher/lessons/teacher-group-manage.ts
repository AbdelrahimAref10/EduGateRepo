import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AddGroupMemberRequest,
  DayOfWeek,
  LessonGroupDto,
  LessonGroupSessionDto,
  LessonStudentDto,
  LessonsClient,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-teacher-group-manage',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink],
  templateUrl: './teacher-group-manage.html',
  styleUrl: './teacher-group-manage.css',
})
export class TeacherGroupManageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly i18n = inject(TranslationService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly lessonId = signal(0);
  readonly groupId = signal(0);
  readonly loading = signal(false);
  readonly loadingSessions = signal(false);
  readonly endingGroup = signal(false);
  readonly deletingGroup = signal(false);
  readonly startingSessionId = signal<number | null>(null);
  readonly endingSessionId = signal<number | null>(null);
  readonly selectingStudentId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly group = signal<LessonGroupDto | null>(null);
  readonly sessions = signal<LessonGroupSessionDto[]>([]);
  readonly lessonStudents = signal<LessonStudentDto[]>([]);

  readonly selectableStudents = computed(() => {
    const groupId = this.groupId();
    return (this.lessonStudents() ?? []).filter((s) => {
      if (s.status !== 'Confirmed') return false;
      if (s.assignedGroupId != null && s.assignedGroupId !== groupId) return false;
      return true;
    });
  });

  ngOnInit(): void {
    this.lessonId.set(Number(this.route.snapshot.paramMap.get('lessonId')));
    this.groupId.set(Number(this.route.snapshot.paramMap.get('groupId')));
    this.loadAll();
  }

  loadGroup(): void {
    this.loadAll();
  }

  loadAll(): void {
    const lessonId = this.lessonId();
    const groupId = this.groupId();
    if (!lessonId || !groupId) {
      this.error.set('Group not found.');
      return;
    }

    this.error.set(null);
    this.loadGroupDetails(lessonId, groupId);
    this.loadSessions(lessonId, groupId);
  }

  private loadGroupDetails(lessonId: number, groupId: number): void {
    this.loading.set(true);
    this.lessonsApi.getGroup(lessonId, groupId).subscribe({
      next: (data) => {
        this.group.set(data.group);
        this.lessonStudents.set(data.students ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load group.');
      },
    });
  }

  private loadSessions(lessonId = this.lessonId(), groupId = this.groupId()): void {
    if (!lessonId || !groupId) return;

    this.loadingSessions.set(true);
    this.lessonsApi.getGroupSessions(lessonId, groupId).subscribe({
      next: (rows) => {
        this.sessions.set(rows ?? []);
        this.loadingSessions.set(false);
      },
      error: (err) => {
        this.loadingSessions.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load sessions.');
      },
    });
  }

  isMember(student: LessonStudentDto): boolean {
    return student.assignedGroupId === this.groupId();
  }

  selectStudent(student: LessonStudentDto): void {
    if (this.group()?.hasEnded) {
      this.error.set(this.i18n.t('lessons.groupEndedMsg'));
      return;
    }
    if (this.isMember(student) || this.selectingStudentId() !== null) return;
    if (!student.studentId) {
      this.error.set(this.i18n.t('lessons.studentCodeRequired'));
      return;
    }

    this.selectingStudentId.set(student.studentId);
    this.error.set(null);
    this.success.set(null);

    this.lessonsApi
      .addGroupMember(
        this.lessonId(),
        this.groupId(),
        new AddGroupMemberRequest({
          studentId: student.studentId,
          studentCode: student.studentCode?.trim() || undefined,
        }),
      )
      .subscribe({
        next: (updated) => {
          this.selectingStudentId.set(null);
          this.group.set(updated);
          this.success.set('memberAdded');
          this.lessonStudents.update((items) =>
            items.map((item) =>
              item.studentId === student.studentId
                ? LessonStudentDto.fromJS({
                    ...item.toJSON(),
                    assignedGroupId: this.groupId(),
                    assignedGroupName: updated.name,
                  })
                : item,
            ),
          );
        },
        error: (err) => {
          this.selectingStudentId.set(null);
          this.error.set(this.readApiError(err, 'Failed to add student.'));
        },
      });
  }

  private readApiError(err: unknown, fallback: string): string {
    const e = err as {
      result?: { detail?: string; title?: string; errors?: Record<string, string[]> };
      message?: string;
    };
    const fromErrors = e?.result?.errors
      ? Object.values(e.result.errors)
          .flat()
          .filter(Boolean)
          .join(' ')
      : '';
    return e?.result?.detail || fromErrors || e?.result?.title || e?.message || fallback;
  }

  endGroup(): void {
    void this.runEndGroup();
  }

  private async runEndGroup(): Promise<void> {
    const group = this.group();
    if (!group?.hasStarted || group.hasEnded) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmEndGroup',
      confirmKey: 'lessons.endGroup',
      tone: 'warning',
    });
    if (!ok) return;

    this.endingGroup.set(true);
    this.error.set(null);

    this.lessonsApi.endGroup(this.lessonId(), this.groupId()).subscribe({
      next: () => {
        this.endingGroup.set(false);
        this.success.set('groupEnded');
        this.loadAll();
      },
      error: (err) => {
        this.endingGroup.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to end group.');
      },
    });
  }

  deleteGroup(): void {
    void this.runDeleteGroup();
  }

  private async runDeleteGroup(): Promise<void> {
    const group = this.group();
    if (!group?.canDelete) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmDeleteGroup',
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.deletingGroup.set(true);
    this.error.set(null);

    this.lessonsApi.deleteGroup(this.lessonId(), this.groupId()).subscribe({
      next: () => {
        this.deletingGroup.set(false);
        void this.router.navigate(['/teacher/lessons', this.lessonId()]);
      },
      error: (err) => {
        this.deletingGroup.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to delete group.');
      },
    });
  }

  startSession(session: LessonGroupSessionDto): void {
    void this.runStartSession(session);
  }

  private async runStartSession(session: LessonGroupSessionDto): Promise<void> {
    if (!session.canStart) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmStartSession',
      confirmKey: 'lessons.startSession',
      tone: 'primary',
    });
    if (!ok) return;

    this.startingSessionId.set(session.id);
    this.error.set(null);

    this.lessonsApi.startSession(this.lessonId(), this.groupId(), session.id).subscribe({
      next: (updated) => {
        this.startingSessionId.set(null);
        void this.router.navigate(['/teacher/classroom', updated.id]);
      },
      error: (err) => {
        this.startingSessionId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to start session.');
      },
    });
  }

  endSession(session: LessonGroupSessionDto): void {
    void this.runEndSession(session);
  }

  private async runEndSession(session: LessonGroupSessionDto): Promise<void> {
    if (!session.hasStarted || session.hasEnded) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmEndSession',
      confirmKey: 'lessons.endSession',
      tone: 'warning',
    });
    if (!ok) return;

    this.endingSessionId.set(session.id);
    this.error.set(null);

    this.lessonsApi.endSession(this.lessonId(), this.groupId(), session.id).subscribe({
      next: (updated) => {
        this.endingSessionId.set(null);
        this.success.set('sessionEnded');
        this.sessions.update((rows) =>
          rows.map((row) => (row.id === updated.id ? updated : row)),
        );
      },
      error: (err) => {
        this.endingSessionId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to end session.');
      },
    });
  }

  openClassroom(session: LessonGroupSessionDto): void {
    if (!session.canOpenClassroom) return;
    void this.router.navigate(['/teacher/classroom', session.id]);
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

  formatSchedule(group: LessonGroupDto): string {
    return (group.dates ?? [])
      .map((d) => `${this.dayLabel(d.dayOfWeek)} ${this.toTimeInput(d.startTime)}`)
      .join(' · ');
  }

  groupStatusKey(group: LessonGroupDto): string {
    if (group.hasEnded) return 'lessons.groupEnded';
    if (group.hasStarted) return 'lessons.groupRunning';
    return 'lessons.groupDraft';
  }

  sessionStatusKey(session: LessonGroupSessionDto): string {
    if (session.hasEnded) return 'lessons.sessionEndedStatus';
    if (session.hasStarted) return 'lessons.sessionRunning';
    return 'lessons.sessionPending';
  }

  toTimeInput(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }
}
