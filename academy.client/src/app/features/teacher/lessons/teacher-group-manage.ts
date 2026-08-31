import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EMPTY, Subject, catchError, forkJoin, of, switchMap } from 'rxjs';
import {
  AddGroupMemberRequest,
  BillingClient,
  ChargeSettlement,
  CreateMakeupSessionRequest,
  DayOfWeek,
  LedgerChargeRowDto,
  LedgerPaymentRowDto,
  LessonGroupDto,
  LessonGroupSessionDto,
  LessonStudentDto,
  LessonsClient,
} from '../../../core/api/academy-api.generated';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { LearningPathApi, TeacherGroupProgressDto } from '../../../core/api/learning-path-api.service';
import { ProgressReportViewComponent } from '../../learning/progress-report-view';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { PaginatorComponent } from '../../../shared/paginator/paginator';

type GroupPanel = 'sessions' | 'members' | 'booked' | 'ledger' | 'progress';

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
    PaginatorComponent,
    ProgressReportViewComponent,
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
  private readonly learningApi = inject(LearningPathApi);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ledger$ = new Subject<void>();
  private readonly studentSheet$ = new Subject<number | null>();

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
  readonly loadingProgress = signal(false);
  readonly progressReady = signal(false);
  readonly groupProgress = signal<TeacherGroupProgressDto | null>(null);
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
  readonly ledgerRows = signal<LedgerChargeRowDto[]>([]);
  readonly ledgerPage = signal(1);
  readonly ledgerPageSize = 10;
  readonly ledgerTotal = signal(0);
  readonly panel = signal<GroupPanel | null>(null);

  readonly studentSheet = signal<{
    studentName: string;
    lessonTitle: string;
    remaining: number;
    charges: LedgerChargeRowDto[];
    payments: LedgerPaymentRowDto[];
  } | null>(null);
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

  constructor() {
    this.ledger$
      .pipe(
        switchMap(() => {
          const lessonId = this.lessonId();
          const groupId = this.groupId();
          if (!lessonId || !groupId) return EMPTY;
          this.loadingLedger.set(true);
          return this.billingApi
            .getOutstanding(
              undefined,
              undefined,
              undefined,
              lessonId,
              groupId,
              undefined,
              undefined,
              undefined,
              undefined,
              this.ledgerPage(),
              this.ledgerPageSize,
            )
            .pipe(
              catchError((err) => {
                this.loadingLedger.set(false);
                this.ledgerReady.set(true);
                this.error.set(this.readApiError(err, 'Failed to load ledger.'));
                return EMPTY;
              }),
            );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((page) => {
        this.ledgerRows.set(page.items ?? []);
        this.ledgerTotal.set(page.totalCount ?? 0);
        this.ledgerReady.set(true);
        this.loadingLedger.set(false);
      });

    this.studentSheet$
      .pipe(
        switchMap((studentId) => {
          if (!studentId) {
            this.studentSheet.set(null);
            this.loadingStudentLedger.set(false);
            return EMPTY;
          }
          this.loadingStudentLedger.set(true);
          const lessonId = this.lessonId();
          return forkJoin({
            outstanding: this.billingApi.getStudentOutstanding(studentId),
            payments: this.billingApi.getPayments(
              studentId,
              undefined,
              undefined,
              lessonId,
              this.groupId() || undefined,
              undefined,
              undefined,
              undefined,
              1,
              10,
            ),
          }).pipe(
            catchError((err) => {
              this.loadingStudentLedger.set(false);
              this.error.set(this.readApiError(err, 'Failed to load student ledger.'));
              return of(null);
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        this.loadingStudentLedger.set(false);
        if (!data) return;
        const lesson = data.outstanding.lessons.find((item) => item.lessonId === this.lessonId());
        this.studentSheet.set({
          studentName: data.outstanding.studentName,
          lessonTitle: lesson?.lessonTitle ?? '',
          remaining: lesson?.remaining ?? 0,
          charges: lesson?.charges ?? [],
          payments: data.payments.items ?? [],
        });
      });
  }

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
        if (this.panel() === 'progress') this.ensureProgress(true);
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
    if (next === 'progress') this.ensureProgress();
  }

  ensureProgress(force = false): void {
    if (!force && this.progressReady()) return;
    const lessonId = this.lessonId();
    const groupId = this.groupId();
    if (!lessonId || !groupId) return;
    this.loadingProgress.set(true);
    this.learningApi.getTeacherGroupProgress(lessonId, groupId).subscribe({
      next: (data) => {
        this.groupProgress.set(data);
        this.progressReady.set(true);
        this.loadingProgress.set(false);
      },
      error: (err) => {
        this.loadingProgress.set(false);
        this.error.set(this.readApiError(err, 'Failed to load progress.'));
      },
    });
  }

  ensureLedger(force = false): void {
    if (!force && this.ledgerReady()) return;
    this.ledger$.next();
  }

  onLedgerPageChange(page: number): void {
    this.ledgerPage.set(page);
    this.ledgerReady.set(false);
    this.ledger$.next();
  }

  openStudentLedger(studentId: number): void {
    this.studentSheet$.next(studentId);
  }

  closeStudentLedger(): void {
    this.studentSheet$.next(null);
  }

  downloadReceipt(payment: { id: number; receiptNumber?: number }): void {
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
