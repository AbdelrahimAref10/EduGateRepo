import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  AddGroupMemberRequest,
  AddLessonStudentRequest,
  AreaDto,
  CreateLessonGroupRequest,
  DayOfWeek,
  LessonGroupDateInputDto,
  LessonGroupDto,
  LessonManageDto,
  LessonsClient,
  UpdateLessonGroupRequest,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-teacher-lesson-manage',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, DatePipe, RouterLink],
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
  readonly loading = signal(false);
  readonly loadingAreas = signal(false);
  readonly savingGroup = signal(false);
  readonly endingGroupId = signal<number | null>(null);
  readonly deletingGroupId = signal<number | null>(null);
  readonly addingMemberGroupId = signal<number | null>(null);
  readonly removingMemberKey = signal<string | null>(null);
  readonly addingStudent = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly manage = signal<LessonManageDto | null>(null);
  readonly cityAreas = signal<AreaDto[]>([]);
  readonly editingGroupId = signal<number | null>(null);
  readonly memberCodeDraft = signal<Record<number, string>>({});
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
    const id = Number(this.route.snapshot.paramMap.get('lessonId'));
    this.lessonId.set(id);
    this.loadCityAreas();
    this.loadManage();
  }

  loadManage(): void {
    const id = this.lessonId();
    if (!id) {
      this.error.set('Lesson not found.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.lessonsApi.getLessonManage(id).subscribe({
      next: (data) => {
        this.manage.set(data);
        this.loading.set(false);
        if (!this.editingGroupId() && data.lesson?.areaId) {
          this.groupForm.controls.areaId.setValue(data.lesson.areaId);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load lesson.');
      },
    });
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
        next: () => {
          this.addingStudent.set(false);
          this.lessonStudentCode.set('');
          this.success.set('studentAdded');
          this.loadManage();
        },
        error: (err) => {
          this.addingStudent.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to add student.');
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
  }

  cancelEditGroup(): void {
    this.editingGroupId.set(null);
    this.resetGroupForm();
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
        next: () => {
          this.savingGroup.set(false);
          this.success.set('groupUpdated');
          this.cancelEditGroup();
          this.loadManage();
        },
        error: (err) => {
          this.savingGroup.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to update group.');
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
      next: () => {
        this.savingGroup.set(false);
        this.success.set('groupCreated');
        this.resetGroupForm();
        this.loadManage();
      },
      error: (err) => {
        this.savingGroup.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to create group.');
      },
    });
  }

  deleteGroup(group: LessonGroupDto): void {
    void this.runDeleteGroup(group);
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
        if (this.editingGroupId() === group.id) this.cancelEditGroup();
        this.loadManage();
      },
      error: (err) => {
        this.deletingGroupId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to delete group.');
      },
    });
  }

  endGroup(group: LessonGroupDto): void {
    void this.runEndGroup(group);
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
      next: () => {
        this.endingGroupId.set(null);
        this.success.set('groupEnded');
        this.loadManage();
      },
      error: (err) => {
        this.endingGroupId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to end group.');
      },
    });
  }

  memberCode(groupId: number): string {
    return this.memberCodeDraft()[groupId] ?? '';
  }

  setMemberCode(groupId: number, value: string): void {
    this.memberCodeDraft.update((draft) => ({ ...draft, [groupId]: value }));
  }

  addMember(group: LessonGroupDto): void {
    if (!group.canEdit) return;

    const code = this.memberCode(group.id).trim();
    if (!code) {
      this.error.set(this.i18n.t('lessons.studentCodeRequired'));
      return;
    }

    this.addingMemberGroupId.set(group.id);
    this.error.set(null);

    this.lessonsApi
      .addGroupMember(this.lessonId(), group.id, new AddGroupMemberRequest({ studentCode: code }))
      .subscribe({
        next: () => {
          this.addingMemberGroupId.set(null);
          this.setMemberCode(group.id, '');
          this.success.set('memberAdded');
          this.loadManage();
        },
        error: (err) => {
          this.addingMemberGroupId.set(null);
          this.error.set(err?.result?.detail || err?.message || 'Failed to add student.');
        },
      });
  }

  removeMember(group: LessonGroupDto, studentId: number): void {
    void this.runRemoveMember(group, studentId);
  }

  private async runRemoveMember(group: LessonGroupDto, studentId: number): Promise<void> {
    if (!group.canEdit) return;

    const ok = await this.confirmDialog.ask({
      messageKey: 'lessons.confirmRemoveMember',
      confirmKey: 'lessons.removeStudent',
      tone: 'danger',
    });
    if (!ok) return;

    const key = `${group.id}:${studentId}`;
    this.removingMemberKey.set(key);
    this.error.set(null);

    this.lessonsApi.removeGroupMember(this.lessonId(), group.id, studentId).subscribe({
      next: () => {
        this.removingMemberKey.set(null);
        this.success.set('memberRemoved');
        this.loadManage();
      },
      error: (err) => {
        this.removingMemberKey.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to remove student.');
      },
    });
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

  private createDateRow() {
    return this.fb.nonNullable.group({
      dayOfWeek: [DayOfWeek.Saturday as DayOfWeek, Validators.required],
      startTime: ['17:00', Validators.required],
    });
  }

  private resetGroupForm(): void {
    const lesson = this.manage()?.lesson;
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

  toTimeInput(value?: string): string {
    if (!value) return '17:00';
    return value.length >= 5 ? value.slice(0, 5) : value;
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

  private loadCityAreas(): void {
    this.loadingAreas.set(true);
    this.lessonsApi.getMyCityAreas().subscribe({
      next: (items) => {
        this.cityAreas.set(items ?? []);
        this.loadingAreas.set(false);
      },
      error: (err) => {
        this.loadingAreas.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load areas.');
      },
    });
  }
}
