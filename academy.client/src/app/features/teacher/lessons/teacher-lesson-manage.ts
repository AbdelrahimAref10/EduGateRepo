import { DatePipe } from '@angular/common';
import { Component, HostListener, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  AddLessonStudentRequest,
  AreaDto,
  CreateLessonGroupRequest,
  DayOfWeek,
  LessonDto,
  LessonGroupDateInputDto,
  LessonGroupDto,
  LessonStudentDto,
  LessonsClient,
  UpdateLessonGroupRequest,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

type LessonPanel = 'groups' | 'students';

@Component({
  selector: 'app-teacher-lesson-manage',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, DatePipe, RouterLink, PageLoaderComponent, UserAvatarComponent],
  templateUrl: './teacher-lesson-manage.html',
  styleUrl: './teacher-lesson-manage.css',
})
export class TeacherLessonManageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly i18n = inject(TranslationService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly DayOfWeek = DayOfWeek;
  readonly weekDays = [
    DayOfWeek.Saturday,
    DayOfWeek.Sunday,
    DayOfWeek.Monday,
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday,
    DayOfWeek.Friday,
  ];

  readonly lessonId = signal(0);
  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly loadingAreas = signal(false);
  readonly loadingGroups = signal(false);
  readonly loadingStudents = signal(false);
  readonly groupsReady = signal(false);
  readonly studentsReady = signal(false);
  readonly savingGroup = signal(false);
  readonly endingGroupId = signal<number | null>(null);
  readonly deletingGroupId = signal<number | null>(null);
  readonly addingStudent = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly lesson = signal<LessonDto | null>(null);
  readonly groups = signal<LessonGroupDto[]>([]);
  readonly students = signal<LessonStudentDto[]>([]);
  readonly cityAreas = signal<AreaDto[]>([]);
  readonly panel = signal<LessonPanel | null>(null);
  readonly editingGroupId = signal<number | null>(null);
  readonly formOpen = signal(false);
  readonly lessonStudentCode = signal('');

  readonly groupForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    areaId: [0, [Validators.required, Validators.min(1)]],
    address: ['', [Validators.required, Validators.maxLength(500)]],
    notes: [''],
    maxCapacity: [null as number | null],
    periodStartDate: ['', Validators.required],
    periodEndDate: ['', Validators.required],
    dates: this.fb.array([this.createDateRow()]),
  });

  get dates(): FormArray {
    return this.groupForm.controls.dates;
  }

  ngOnInit(): void {
    this.lessonId.set(Number(this.route.snapshot.paramMap.get('lessonId')));
    this.loadLesson();
  }

  loadLesson(): void {
    const id = this.lessonId();
    if (!id) {
      this.loading.set(false);
      this.ready.set(true);
      this.error.set('Lesson not found.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.groupsReady.set(false);
    this.studentsReady.set(false);
    this.groups.set([]);
    this.students.set([]);

    this.lessonsApi.getLessonManage(id).subscribe({
      next: (data) => {
        this.lesson.set(data.lesson);
        this.loading.set(false);
        this.ready.set(true);
        if (this.panel() === 'groups') this.ensureGroups(true);
        if (this.panel() === 'students') this.ensureStudents(true);
      },
      error: (err) => {
        this.loading.set(false);
        this.ready.set(true);
        this.error.set(this.apiError(err, 'Failed to load lesson.'));
      },
    });
  }

  togglePanel(next: LessonPanel): void {
    if (this.panel() === next) {
      this.panel.set(null);
      return;
    }

    this.panel.set(next);
    this.success.set(null);
    if (next === 'groups') this.ensureGroups();
    if (next === 'students') this.ensureStudents();
  }

  openCreate(): void {
    this.editingGroupId.set(null);
    this.resetGroupForm();
    this.error.set(null);
    this.success.set(null);
    this.formOpen.set(true);
    this.ensureAreas();
  }

  closeForm(): void {
    if (this.savingGroup()) return;
    this.formOpen.set(false);
    this.editingGroupId.set(null);
    this.resetGroupForm();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.formOpen()) this.closeForm();
  }

  addLessonStudent(): void {
    const code = this.lessonStudentCode().trim();
    if (!code) {
      this.error.set(this.i18n.t('lessons.studentCodeRequired'));
      return;
    }

    this.addingStudent.set(true);
    this.error.set(null);
    this.success.set(null);

    this.lessonsApi
      .addLessonStudent(this.lessonId(), new AddLessonStudentRequest({ studentCode: code }))
      .subscribe({
        next: (created) => {
          this.addingStudent.set(false);
          this.lessonStudentCode.set('');
          this.success.set('studentAdded');
          this.students.update((items) => [created, ...items]);
          this.studentsReady.set(true);
          this.bumpLessonCounts({ bookings: 1, confirmed: created.status === 'Confirmed' ? 1 : 0 });
        },
        error: (err) => {
          this.addingStudent.set(false);
          this.error.set(this.apiError(err, 'Failed to add student.'));
        },
      });
  }

  addDateRow(): void {
    this.dates.push(this.createDateRow());
  }

  removeDateRow(index: number): void {
    if (this.dates.length <= 1) return;
    this.dates.removeAt(index);
  }

  startEditGroup(group: LessonGroupDto): void {
    if (!group.canEdit) return;

    this.editingGroupId.set(group.id);
    this.success.set(null);
    this.error.set(null);
    this.ensureAreas();

    while (this.dates.length) this.dates.removeAt(0);
    const rows = group.dates?.length
      ? group.dates
      : [{ dayOfWeek: DayOfWeek.Saturday, startTime: '17:00' }];

    for (const row of rows) {
      this.dates.push(
        this.fb.nonNullable.group({
          dayOfWeek: [Number(row.dayOfWeek) as DayOfWeek, Validators.required],
          startTime: [this.toTimeInput(row.startTime), Validators.required],
        }),
      );
    }

    this.groupForm.patchValue({
      name: group.name,
      areaId: group.areaId,
      address: group.address,
      notes: group.notes ?? '',
      maxCapacity: group.maxCapacity ?? null,
      periodStartDate: this.toDateInput(group.periodStartDate),
      periodEndDate: this.toDateInput(group.periodEndDate),
    });
    this.formOpen.set(true);
  }

  submitGroup(): void {
    this.error.set(null);
    this.success.set(null);

    if (this.groupForm.invalid || this.dates.length === 0) {
      this.groupForm.markAllAsTouched();
      return;
    }

    const value = this.groupForm.getRawValue();
    const dates = value.dates.map(
      (row) =>
        new LessonGroupDateInputDto({
          dayOfWeek: Number(row.dayOfWeek) as DayOfWeek,
          startTime: this.toTimeApi(row.startTime),
        }),
    );

    const periodStartDate = this.parseDate(value.periodStartDate);
    const periodEndDate = this.parseDate(value.periodEndDate);
    if (!periodStartDate || !periodEndDate) {
      this.error.set(this.i18n.t('lessons.periodRequired'));
      return;
    }

    const editingId = this.editingGroupId();
    this.savingGroup.set(true);

    if (editingId) {
      const request = new UpdateLessonGroupRequest({
        name: value.name.trim(),
        dates,
        periodStartDate,
        periodEndDate,
        areaId: value.areaId,
        address: value.address.trim(),
        notes: value.notes.trim() || undefined,
        maxCapacity: value.maxCapacity ?? undefined,
      });

      this.lessonsApi.updateGroup(this.lessonId(), editingId, request).subscribe({
        next: (updated) => {
          this.savingGroup.set(false);
          this.success.set('groupUpdated');
          this.groups.update((items) => items.map((item) => (item.id === updated.id ? updated : item)));
          this.closeForm();
        },
        error: (err) => {
          this.savingGroup.set(false);
          this.error.set(this.apiError(err, 'Failed to update group.'));
        },
      });
      return;
    }

    const request = new CreateLessonGroupRequest({
      name: value.name.trim(),
      dates,
      periodStartDate,
      periodEndDate,
      areaId: value.areaId || undefined,
      address: value.address.trim(),
      notes: value.notes.trim() || undefined,
      maxCapacity: value.maxCapacity ?? undefined,
    });

    this.lessonsApi.createGroup(this.lessonId(), request).subscribe({
      next: (created) => {
        this.savingGroup.set(false);
        this.success.set('groupCreated');
        this.groups.update((items) => [...items, created]);
        this.groupsReady.set(true);
        this.bumpLessonCounts({ groups: 1 });
        this.closeForm();
        this.panel.set('groups');
      },
      error: (err) => {
        this.savingGroup.set(false);
        this.error.set(this.apiError(err, 'Failed to create group.'));
      },
    });
  }

  deleteGroup(group: LessonGroupDto): void {
    void this.runDeleteGroup(group);
  }

  endGroup(group: LessonGroupDto): void {
    void this.runEndGroup(group);
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

  bookingStatusKey(status: string): string {
    switch (status) {
      case 'Confirmed':
        return 'lessons.bookingConfirmed';
      case 'Rejected':
        return 'lessons.bookingRejected';
      default:
        return 'lessons.bookingPending';
    }
  }

  toTimeInput(value?: string): string {
    if (!value) return '17:00';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  private ensureGroups(force = false): void {
    if ((this.groupsReady() && !force) || this.loadingGroups()) return;

    this.loadingGroups.set(true);
    this.lessonsApi.getLessonGroups(this.lessonId()).subscribe({
      next: (items) => {
        this.groups.set(items ?? []);
        this.loadingGroups.set(false);
        this.groupsReady.set(true);
      },
      error: (err) => {
        this.loadingGroups.set(false);
        this.error.set(this.apiError(err, 'Failed to load groups.'));
      },
    });
  }

  private ensureStudents(force = false): void {
    if ((this.studentsReady() && !force) || this.loadingStudents()) return;

    this.loadingStudents.set(true);
    this.lessonsApi.getLessonStudents(this.lessonId()).subscribe({
      next: (items) => {
        this.students.set(items ?? []);
        this.loadingStudents.set(false);
        this.studentsReady.set(true);
      },
      error: (err) => {
        this.loadingStudents.set(false);
        this.error.set(this.apiError(err, 'Failed to load students.'));
      },
    });
  }

  private ensureAreas(): void {
    if (this.cityAreas().length || this.loadingAreas()) return;

    this.loadingAreas.set(true);
    this.lessonsApi.getMyCityAreas().subscribe({
      next: (items) => {
        this.cityAreas.set(items ?? []);
        this.loadingAreas.set(false);
      },
      error: (err) => {
        this.loadingAreas.set(false);
        this.error.set(this.apiError(err, 'Failed to load areas.'));
      },
    });
  }

  private async runDeleteGroup(group: LessonGroupDto): Promise<void> {
    if (!group.canDelete) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmDeleteGroup',
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.deletingGroupId.set(group.id);
    this.error.set(null);

    this.lessonsApi.deleteGroup(this.lessonId(), group.id).subscribe({
      next: () => {
        this.deletingGroupId.set(null);
        this.success.set('groupDeleted');
        if (this.editingGroupId() === group.id) this.closeForm();
        this.groups.update((items) => items.filter((item) => item.id !== group.id));
        this.bumpLessonCounts({ groups: -1 });
      },
      error: (err) => {
        this.deletingGroupId.set(null);
        this.error.set(this.apiError(err, 'Failed to delete group.'));
      },
    });
  }

  private async runEndGroup(group: LessonGroupDto): Promise<void> {
    if (!group.hasStarted || group.hasEnded) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmEndGroup',
      confirmKey: 'lessons.endGroup',
      tone: 'warning',
    });
    if (!ok) return;

    this.endingGroupId.set(group.id);
    this.error.set(null);

    this.lessonsApi.endGroup(this.lessonId(), group.id).subscribe({
      next: (updated) => {
        this.endingGroupId.set(null);
        this.success.set('groupEnded');
        this.groups.update((items) => items.map((item) => (item.id === updated.id ? updated : item)));
      },
      error: (err) => {
        this.endingGroupId.set(null);
        this.error.set(this.apiError(err, 'Failed to end group.'));
      },
    });
  }

  private bumpLessonCounts(delta: { groups?: number; bookings?: number; confirmed?: number }): void {
    this.lesson.update((current) => {
      if (!current) return current;
      return {
        ...current,
        groupsCount: Math.max(0, (current.groupsCount ?? 0) + (delta.groups ?? 0)),
        bookingsCount: Math.max(0, (current.bookingsCount ?? 0) + (delta.bookings ?? 0)),
        confirmedBookingsCount: Math.max(0, (current.confirmedBookingsCount ?? 0) + (delta.confirmed ?? 0)),
      } as LessonDto;
    });
  }

  private createDateRow() {
    return this.fb.nonNullable.group({
      dayOfWeek: [DayOfWeek.Saturday as DayOfWeek, Validators.required],
      startTime: ['17:00', Validators.required],
    });
  }

  private resetGroupForm(): void {
    const lesson = this.lesson();
    while (this.dates.length) this.dates.removeAt(0);
    this.dates.push(this.createDateRow());
    this.groupForm.reset({
      name: '',
      areaId: lesson?.areaId ?? 0,
      address: '',
      notes: '',
      maxCapacity: null,
      periodStartDate: '',
      periodEndDate: '',
    });
  }

  private toTimeApi(value: string): string {
    if (!value) return '00:00:00';
    return value.length === 5 ? `${value}:00` : value;
  }

  private toDateInput(value?: Date | string): string {
    if (!value) return '';
    const date = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(date.getTime())) return '';
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private parseDate(value: string): Date | undefined {
    if (!value) return undefined;
    const [y, m, d] = value.split('-').map(Number);
    if (!y || !m || !d) return undefined;
    return new Date(y, m - 1, d);
  }

  private apiError(err: any, fallback: string): string {
    return err?.result?.detail || err?.message || fallback;
  }
}
