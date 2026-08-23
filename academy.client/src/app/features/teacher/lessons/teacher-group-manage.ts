import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AddGroupMemberRequest,
  BillingClient,
  ChargeSettlement,
  CreateMakeupSessionRequest,
  DayOfWeek,
  LedgerStudentRowDto,
  LessonGroupDto,
  LessonGroupSessionDto,
  LessonStudentDto,
  LessonsClient,
  PaymentDto,
  StudentLessonLedgerDto,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';

type GroupPanel = 'sessions' | 'members' | 'booked' | 'ledger';

@Component({
  selector: 'app-teacher-group-manage',
  standalone: true,
  imports: [
    TranslatePipe,
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    RouterLink,
    UserAvatarComponent,
    PageLoaderComponent,
  ],
  templateUrl: './teacher-group-manage.html',
  styleUrl: './teacher-group-manage.css',
})
export class TeacherGroupManageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly lessonsApi = inject(LessonsClient);
  private readonly billingApi = inject(BillingClient);
  private readonly i18n = inject(TranslationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly destroyRef = inject(DestroyRef);

  readonly lessonId = signal(0);
  readonly groupId = signal(0);
  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly loadingSessions = signal(false);
  readonly sessionsReady = signal(false);
  readonly loadingUnassigned = signal(false);
  readonly unassignedReady = signal(false);
  readonly loadingLedger = signal(false);
  readonly ledgerReady = signal(false);
  readonly endingGroup = signal(false);
  readonly deletingGroup = signal(false);
  readonly startingSessionId = signal<number | null>(null);
  readonly endingSessionId = signal<number | null>(null);
  readonly selectingStudentId = signal<number | null>(null);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly group = signal<LessonGroupDto | null>(null);
  readonly lessonBillingType = signal<'PerSession' | 'Monthly' | null>(null);
  readonly lessonSessionPrice = signal<number | null>(null);
  readonly lessonMonthlyPrice = signal<number | null>(null);
  readonly sessions = signal<LessonGroupSessionDto[]>([]);
  readonly unassignedStudents = signal<LessonStudentDto[]>([]);
  readonly ledgerRows = signal<LedgerStudentRowDto[]>([]);
  readonly panel = signal<GroupPanel | null>(null);

  readonly studentLedger = signal<StudentLessonLedgerDto | null>(null);
  readonly loadingStudentLedger = signal(false);
  readonly makeupOpen = signal(false);
  readonly savingMakeup = signal(false);
  readonly selectedMakeupIds = signal<Set<number>>(new Set());

  readonly settlements = [
    { value: ChargeSettlement.Standalone, key: 'billing.settlementStandalone' },
    { value: ChargeSettlement.CurrentCycle, key: 'billing.settlementCurrent' },
    { value: ChargeSettlement.NextCycle, key: 'billing.settlementNext' },
  ] as const;

  readonly isMonthlyBilling = () => this.lessonBillingType() === 'Monthly';
  readonly isPerSessionBilling = () => this.lessonBillingType() === 'PerSession';

  readonly makeupForm = this.fb.nonNullable.group({
    sessionDate: ['', Validators.required],
    startTime: ['16:00', Validators.required],
    topic: [''],
    makeupForSessionId: [null as number | null],
    isFree: [true],
    amount: [null as number | null],
    settlement: [ChargeSettlement.Standalone as ChargeSettlement],
  });

  ngOnInit(): void {
    this.lessonId.set(Number(this.route.snapshot.paramMap.get('lessonId')));
    this.groupId.set(Number(this.route.snapshot.paramMap.get('groupId')));
    this.makeupForm.controls.isFree.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((free) => this.onMakeupFreeChanged(!!free));
    this.loadGroup();
    this.loadLessonBilling();
  }

  loadLessonBilling(): void {
    const lessonId = this.lessonId();
    if (!lessonId) return;
    this.lessonsApi
      .getLessonManage(lessonId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          const type = String(data.lesson?.billingType ?? '');
          this.lessonBillingType.set(type === 'Monthly' ? 'Monthly' : type === 'PerSession' ? 'PerSession' : null);
          this.lessonSessionPrice.set(data.lesson?.sessionPrice ?? null);
          this.lessonMonthlyPrice.set(data.lesson?.monthlyPrice ?? null);
        },
        error: () => undefined,
      });
  }

  loadGroup(): void {
    const lessonId = this.lessonId();
    const groupId = this.groupId();
    if (!lessonId || !groupId) {
      this.loading.set(false);
      this.ready.set(true);
      this.error.set('Group not found.');
      return;
    }

    this.error.set(null);
    this.loading.set(true);
    this.sessionsReady.set(false);
    this.sessions.set([]);
    this.unassignedReady.set(false);
    this.unassignedStudents.set([]);
    this.ledgerReady.set(false);
    this.ledgerRows.set([]);

    this.lessonsApi.getGroup(lessonId, groupId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        this.group.set(data);
        this.loading.set(false);
        this.ready.set(true);
        if (this.panel() === 'sessions') this.ensureSessions(true);
        if (this.panel() === 'booked') this.ensureUnassignedStudents(true);
        if (this.panel() === 'ledger') this.ensureLedger(true);
      },
      error: (err) => {
        this.loading.set(false);
        this.ready.set(true);
        this.error.set(this.readApiError(err, 'Failed to load group.'));
      },
    });
  }

  togglePanel(next: GroupPanel): void {
    if (this.panel() === next) {
      this.panel.set(null);
      return;
    }

    this.panel.set(next);
    this.success.set(null);
    if (next === 'sessions') this.ensureSessions();
    if (next === 'booked') this.ensureUnassignedStudents();
    if (next === 'ledger') this.ensureLedger();
  }

  ensureLedger(force = false): void {
    if (!force && this.ledgerReady()) return;
    const lessonId = this.lessonId();
    const groupId = this.groupId();
    if (!lessonId || !groupId || this.loadingLedger()) return;

    this.loadingLedger.set(true);
    this.billingApi
      .getGroupLedger(lessonId, groupId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (rows) => {
          this.ledgerRows.set(rows ?? []);
          this.ledgerReady.set(true);
          this.loadingLedger.set(false);
        },
        error: (err) => {
          this.loadingLedger.set(false);
          this.ledgerReady.set(true);
          this.error.set(this.readApiError(err, 'Failed to load ledger.'));
        },
      });
  }

  openStudentLedger(studentId: number): void {
    this.loadingStudentLedger.set(true);
    this.studentLedger.set(null);
    this.billingApi
      .getStudentLedger(this.lessonId(), studentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.studentLedger.set(data);
          this.loadingStudentLedger.set(false);
        },
        error: (err) => {
          this.loadingStudentLedger.set(false);
          this.error.set(this.readApiError(err, 'Failed to load student ledger.'));
        },
      });
  }

  closeStudentLedger(): void {
    this.studentLedger.set(null);
  }

  downloadReceipt(payment: PaymentDto): void {
    this.billingApi.downloadReceipt(payment.id).subscribe({
      next: (file) => this.saveBlob(file.data, file.fileName || `receipt-${payment.receiptNumber}.pdf`),
      error: (err) => this.error.set(this.readApiError(err, 'Failed to download receipt.')),
    });
  }

  openMakeup(): void {
    this.ensureSessions();
    const members = this.group()?.members ?? [];
    this.selectedMakeupIds.set(new Set(members.map((m) => m.studentId)));
    const sessionPrice = this.lessonSessionPrice();
    this.makeupForm.reset({
      sessionDate: '',
      startTime: '16:00',
      topic: '',
      makeupForSessionId: null,
      isFree: true,
      amount: this.isPerSessionBilling() ? sessionPrice : null,
      settlement: ChargeSettlement.Standalone,
    });
    this.makeupOpen.set(true);
  }

  private onMakeupFreeChanged(free: boolean): void {
    if (free) {
      this.makeupForm.controls.amount.setValue(null);
      return;
    }
    if (this.isPerSessionBilling()) {
      this.makeupForm.controls.amount.setValue(this.lessonSessionPrice());
      this.makeupForm.controls.settlement.setValue(ChargeSettlement.Standalone);
    }
  }

  closeMakeup(): void {
    this.makeupOpen.set(false);
  }

  toggleMakeupStudent(studentId: number): void {
    this.selectedMakeupIds.update((set) => {
      const next = new Set(set);
      if (next.has(studentId)) next.delete(studentId);
      else next.add(studentId);
      return next;
    });
  }

  isMakeupSelected(studentId: number): boolean {
    return this.selectedMakeupIds().has(studentId);
  }

  submitMakeup(): void {
    if (this.makeupForm.invalid) {
      this.makeupForm.markAllAsTouched();
      return;
    }
    const ids = [...this.selectedMakeupIds()];
    if (!ids.length) {
      this.error.set(this.i18n.t('billing.makeupHint'));
      return;
    }

    const value = this.makeupForm.getRawValue();
    this.savingMakeup.set(true);
    this.error.set(null);

    const perSession = this.isPerSessionBilling();
    const amount = value.isFree
      ? undefined
      : perSession
        ? this.lessonSessionPrice() ?? undefined
        : value.amount ?? undefined;

    const request = new CreateMakeupSessionRequest({
      sessionDate: new Date(value.sessionDate),
      startTime: value.startTime.length === 5 ? `${value.startTime}:00` : value.startTime,
      topic: value.topic.trim() || undefined,
      makeupForSessionId: value.makeupForSessionId || undefined,
      studentIds: ids,
      isFree: value.isFree,
      amount,
      settlement: value.isFree
        ? ChargeSettlement.None
        : perSession
          ? ChargeSettlement.Standalone
          : value.settlement,
    });

    this.billingApi
      .createMakeup(this.lessonId(), this.groupId(), request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.savingMakeup.set(false);
          this.closeMakeup();
          this.success.set('makeupCreated');
          this.sessionsReady.set(false);
          this.ensureSessions(true);
          this.ledgerReady.set(false);
          if (this.panel() === 'ledger') this.ensureLedger(true);
        },
        error: (err) => {
          this.savingMakeup.set(false);
          this.error.set(this.readApiError(err, 'Failed to create makeup session.'));
        },
      });
  }

  chargeTypeKey(type?: string): string {
    switch (type) {
      case 'MonthlyCycle':
        return 'billing.monthlyCycle';
      case 'Makeup':
        return 'billing.makeupCharge';
      default:
        return 'billing.sessionCharge';
    }
  }

  private saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
  }

  selectStudent(student: LessonStudentDto): void {
    if (this.group()?.hasEnded) {
      this.error.set(this.i18n.t('lessons.groupEndedMsg'));
      return;
    }
    if (this.selectingStudentId() !== null) return;
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
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.selectingStudentId.set(null);
          this.group.set(updated);
          this.success.set('memberAdded');
          this.unassignedStudents.update((items) =>
            items.filter((item) => item.studentId !== student.studentId),
          );
        },
        error: (err) => {
          this.selectingStudentId.set(null);
          this.error.set(this.readApiError(err, 'Failed to add student.'));
        },
      });
  }

  endGroup(): void {
    void this.runEndGroup();
  }

  deleteGroup(): void {
    void this.runDeleteGroup();
  }

  startSession(session: LessonGroupSessionDto): void {
    void this.runStartSession(session);
  }

  endSession(session: LessonGroupSessionDto): void {
    void this.runEndSession(session);
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

  private ensureUnassignedStudents(force = false): void {
    if (!force && this.unassignedReady()) return;

    const lessonId = this.lessonId();
    if (!lessonId || this.loadingUnassigned()) return;

    this.loadingUnassigned.set(true);
    this.lessonsApi
      .getUnassignedLessonStudents(lessonId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (rows) => {
          this.unassignedStudents.set(rows ?? []);
          this.unassignedReady.set(true);
          this.loadingUnassigned.set(false);
        },
        error: (err) => {
          this.loadingUnassigned.set(false);
          this.error.set(this.readApiError(err, 'Failed to load students.'));
        },
      });
  }

  private ensureSessions(force = false): void {
    if (!force && this.sessionsReady()) return;

    const lessonId = this.lessonId();
    const groupId = this.groupId();
    if (!lessonId || !groupId || this.loadingSessions()) return;

    this.loadingSessions.set(true);
    this.lessonsApi
      .getGroupSessions(lessonId, groupId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (rows) => {
          this.sessions.set(rows ?? []);
          this.sessionsReady.set(true);
          this.loadingSessions.set(false);
        },
        error: (err) => {
          this.loadingSessions.set(false);
          this.error.set(this.readApiError(err, 'Failed to load sessions.'));
        },
      });
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

    this.lessonsApi.endGroup(this.lessonId(), this.groupId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.endingGroup.set(false);
        this.success.set('groupEnded');
        this.loadGroup();
      },
      error: (err) => {
        this.endingGroup.set(false);
        this.error.set(this.readApiError(err, 'Failed to end group.'));
      },
    });
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

    this.lessonsApi.deleteGroup(this.lessonId(), this.groupId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.deletingGroup.set(false);
        void this.router.navigate(['/teacher/lessons', this.lessonId()]);
      },
      error: (err) => {
        this.deletingGroup.set(false);
        this.error.set(this.readApiError(err, 'Failed to delete group.'));
      },
    });
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

    this.lessonsApi
      .startSession(this.lessonId(), this.groupId(), session.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.startingSessionId.set(null);
          void this.router.navigate(['/teacher/classroom', updated.id]);
        },
        error: (err) => {
          this.startingSessionId.set(null);
          this.error.set(this.readApiError(err, 'Failed to start session.'));
        },
      });
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

    this.lessonsApi
      .endSession(this.lessonId(), this.groupId(), session.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.endingSessionId.set(null);
          this.success.set('sessionEnded');
          this.sessions.update((rows) =>
            rows.map((row) => (row.id === updated.id ? updated : row)),
          );
        },
        error: (err) => {
          this.endingSessionId.set(null);
          this.error.set(this.readApiError(err, 'Failed to end session.'));
        },
      });
  }

  private readApiError(err: unknown, fallback: string): string {
    const e = err as {
      result?: { detail?: string; title?: string; errors?: Record<string, string[]> };
      message?: string;
    };
    const fromErrors = e?.result?.errors
      ? Object.values(e.result.errors).flat().filter(Boolean).join(' ')
      : '';
    return e?.result?.detail || fromErrors || e?.result?.title || e?.message || fallback;
  }
}
