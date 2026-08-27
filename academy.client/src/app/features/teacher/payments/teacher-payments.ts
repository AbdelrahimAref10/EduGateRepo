import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  EMPTY,
  Observable,
  Subject,
  catchError,
  concatMap,
  of,
  switchMap,
  tap,
} from 'rxjs';
import {
  BillingClient,
  BillingStudentSearchDto,
  LedgerChargeDetailDto,
  LedgerChargeRowDto,
  LedgerFilterOptionDto,
  LedgerFilterSessionDto,
  LedgerPaymentDetailDto,
  LedgerPaymentRowDto,
  LedgerTransactionDto,
  PaymentMethod,
  RecordTeacherPaymentRequest,
  StudentOutstandingDto,
  StudentOutstandingLessonDto,
  TeacherBillingSummaryDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { PaginatorComponent } from '../../../shared/paginator/paginator';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

type LedgerSection = 'outstanding' | 'charges' | 'payments' | 'transactions';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-teacher-payments',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    TranslatePipe,
    PageLoaderComponent,
    PaginatorComponent,
    UserAvatarComponent,
  ],
  templateUrl: './teacher-payments.html',
  styleUrl: './teacher-payments.css',
})
export class TeacherPaymentsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly billingApi = inject(BillingClient);
  private readonly destroyRef = inject(DestroyRef);
  private readonly list$ = new Subject<void>();

  readonly pageSize = PAGE_SIZE;
  readonly paymentMethods = [
    { value: PaymentMethod.Cash, key: 'billing.methodCash' },
    { value: PaymentMethod.VodafoneCash, key: 'billing.methodVodafone' },
    { value: PaymentMethod.InstaPay, key: 'billing.methodInstaPay' },
    { value: PaymentMethod.Other, key: 'billing.methodOther' },
  ];
  readonly sections: { id: LedgerSection; key: string; hint: string; icon: string; tone: string }[] = [
    {
      id: 'outstanding',
      key: 'teacherPayments.tabDue',
      hint: 'teacherPayments.stillDue',
      icon: 'M12 5v14M5 12h14',
      tone: 'flex size-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-danger to-rose-500 text-white',
    },
    {
      id: 'charges',
      key: 'teacherPayments.tabInvoices',
      hint: 'teacherPayments.chargesTotal',
      icon: 'M4 19h16M6 16V9l6-4 6 4v7',
      tone: 'flex size-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-warning to-orange-500 text-white',
    },
    {
      id: 'payments',
      key: 'teacherPayments.tabCash',
      hint: 'teacherPayments.paymentsTotal',
      icon: 'M4 12h16M12 4v16',
      tone: 'flex size-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-primary-500 to-primary-700 text-white',
    },
    {
      id: 'transactions',
      key: 'teacherPayments.tabJournal',
      hint: 'teacherPayments.entry',
      icon: 'M4 12h16M14 7l5 5-5 5',
      tone: 'flex size-14 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-navy-600 to-navy-800 text-white',
    },
  ];

  readonly searching = signal(false);
  readonly collectSearching = signal(false);
  readonly studentNotFound = signal(false);
  readonly collectSearchError = signal<'empty' | 'none' | null>(null);
  readonly loadingList = signal(false);
  readonly loadingStages = signal(false);
  readonly loadingLessons = signal(false);
  readonly loadingGroups = signal(false);
  readonly loadingSessions = signal(false);
  readonly collecting = signal(false);
  readonly downloading = signal(false);
  readonly loadingDetail = signal(false);
  readonly loadingOutstanding = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly summary = signal<TeacherBillingSummaryDto | null>(null);
  readonly section = signal<LedgerSection | null>(null);
  readonly page = signal(1);
  readonly totalCount = signal(0);
  readonly transactions = signal<LedgerTransactionDto[]>([]);
  readonly charges = signal<LedgerChargeRowDto[]>([]);
  readonly payments = signal<LedgerPaymentRowDto[]>([]);

  readonly academicYears = signal<LedgerFilterOptionDto[]>([]);
  readonly stages = signal<LedgerFilterOptionDto[]>([]);
  readonly lessons = signal<LedgerFilterOptionDto[]>([]);
  readonly groups = signal<LedgerFilterOptionDto[]>([]);
  readonly sessions = signal<LedgerFilterSessionDto[]>([]);
  readonly academicYearId = signal<number | null>(null);
  readonly educationStageId = signal<number | null>(null);
  readonly lessonId = signal<number | null>(null);
  readonly groupId = signal<number | null>(null);
  readonly sessionId = signal<number | null>(null);
  private readonly appliedStudentId = signal<number | undefined>(undefined);
  private readonly appliedAcademicYearId = signal<number | undefined>(undefined);
  private readonly appliedEducationStageId = signal<number | undefined>(undefined);
  private readonly appliedLessonId = signal<number | undefined>(undefined);
  private readonly appliedGroupId = signal<number | undefined>(undefined);
  private readonly appliedSessionId = signal<number | undefined>(undefined);

  readonly filterQuery = signal('');
  readonly filterHits = signal<BillingStudentSearchDto[]>([]);
  readonly filterStudent = signal<BillingStudentSearchDto | null>(null);
  readonly filterOpen = signal(false);

  readonly collectOpen = signal(false);
  readonly collectQuery = signal('');
  readonly collectHits = signal<BillingStudentSearchDto[]>([]);
  readonly collectOpenList = signal(false);
  readonly collectStudent = signal<BillingStudentSearchDto | null>(null);
  readonly collectOutstanding = signal<StudentOutstandingDto | null>(null);
  readonly collectLessonId = signal<number | null>(null);
  readonly selectedChargeIds = signal<ReadonlySet<number>>(new Set());

  readonly detailOpen = signal(false);
  readonly detailKind = signal<'Charge' | 'Payment'>('Charge');
  readonly chargeDetail = signal<LedgerChargeDetailDto | null>(null);
  readonly paymentDetail = signal<LedgerPaymentDetailDto | null>(null);

  readonly collectForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: [PaymentMethod.Cash, Validators.required],
    note: [''],
  });

  readonly collectLesson = computed(() => {
    const lessonId = this.collectLessonId();
    const sheet = this.collectOutstanding();
    if (!lessonId || !sheet) return null;
    return sheet.lessons.find((item) => item.lessonId === lessonId) ?? null;
  });

  readonly selectedRemaining = computed(() => {
    const lesson = this.collectLesson();
    if (!lesson) return 0;
    const selected = this.selectedChargeIds();
    return lesson.charges
      .filter((c) => selected.has(c.id))
      .reduce((sum, c) => sum + Number(c.remaining || 0), 0);
  });

  readonly listEmpty = computed(() => {
    const section = this.section();
    if (section === 'payments') return this.payments().length === 0;
    if (section === 'transactions') return this.transactions().length === 0;
    return this.charges().length === 0;
  });

  readonly hasActiveFilters = computed(
    () =>
      !!this.filterQuery().trim() ||
      !!this.filterStudent() ||
      !!this.academicYearId() ||
      !!this.educationStageId() ||
      !!this.lessonId() ||
      !!this.groupId() ||
      !!this.sessionId(),
  );

  ngOnInit(): void {
    const q = this.route.snapshot.queryParamMap;
    this.academicYearId.set(Number(q.get('academicYearId') || 0) || null);
    this.educationStageId.set(Number(q.get('educationStageId') || 0) || null);
    this.lessonId.set(Number(q.get('lessonId') || 0) || null);
    this.groupId.set(Number(q.get('groupId') || 0) || null);
    this.sessionId.set(Number(q.get('sessionId') || 0) || null);
    this.commitScopeFilters();
    const studentId = Number(q.get('studentId') || 0) || null;

    this.list$
      .pipe(
        switchMap(() => this.listRequest()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => this.applyList(data));

    this.boot(studentId);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.detailOpen()) {
      this.closeDetail();
      return;
    }
    if (this.collectOpen()) this.closeCollect();
  }

  selectSection(section: LedgerSection): void {
    this.section.set(section);
    this.page.set(1);
    this.loadList();
  }

  onAcademicYearChange(raw: string): void {
    this.academicYearId.set(Number(raw) || null);
    this.educationStageId.set(null);
    this.lessonId.set(null);
    this.groupId.set(null);
    this.sessionId.set(null);
    this.stages.set([]);
    this.lessons.set([]);
    this.groups.set([]);
    this.sessions.set([]);
    if (this.academicYearId()) this.loadStages();
  }

  onStageChange(raw: string): void {
    this.educationStageId.set(Number(raw) || null);
    this.lessonId.set(null);
    this.groupId.set(null);
    this.sessionId.set(null);
    this.lessons.set([]);
    this.groups.set([]);
    this.sessions.set([]);
    if (this.educationStageId()) this.loadLessonsFilter();
  }

  onLessonChange(raw: string): void {
    this.lessonId.set(Number(raw) || null);
    this.groupId.set(null);
    this.sessionId.set(null);
    this.groups.set([]);
    this.sessions.set([]);
    if (this.lessonId()) this.loadGroups();
  }

  onGroupChange(raw: string): void {
    this.groupId.set(Number(raw) || null);
    this.sessionId.set(null);
    this.sessions.set([]);
    if (this.groupId()) this.loadSessions();
  }

  onSessionChange(raw: string): void {
    this.sessionId.set(Number(raw) || null);
  }

  onPageChange(page: number): void {
    this.page.set(page);
    this.reloadOpenSection();
  }

  onFilterSearch(value: string): void {
    this.filterQuery.set(value);
    this.studentNotFound.set(false);
    this.filterHits.set([]);
    this.filterOpen.set(false);
    const picked = this.filterStudent();
    if (picked && value.trim() !== picked.fullName) {
      this.filterStudent.set(null);
    }
  }

  applyFilters(): void {
    this.error.set(null);
    this.studentNotFound.set(false);
    this.filterOpen.set(false);
    this.commitScopeFilters();

    const query = this.filterQuery().trim();
    const picked = this.filterStudent();
    if (!query) {
      this.filterStudent.set(null);
      this.filterHits.set([]);
      this.appliedStudentId.set(undefined);
      this.runAppliedQuery();
      return;
    }

    if (picked && this.studentMatchesQuery(picked, query)) {
      this.appliedStudentId.set(picked.id);
      this.runAppliedQuery();
      return;
    }

    this.searching.set(true);
    this.billingApi
      .searchStudents(query)
      .pipe(
        catchError((err) => {
          this.searching.set(false);
          this.error.set(this.apiError(err, 'Failed to search students.'));
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        this.searching.set(false);
        const hits = this.preferExactStudent(rows ?? [], query);
        if (hits.length === 1) {
          this.filterStudent.set(hits[0]);
          this.filterQuery.set(hits[0].fullName);
          this.filterHits.set([]);
          this.appliedStudentId.set(hits[0].id);
          this.runAppliedQuery();
          return;
        }
        if (hits.length === 0) {
          this.filterStudent.set(null);
          this.filterHits.set([]);
          this.appliedStudentId.set(undefined);
          this.studentNotFound.set(true);
          this.runAppliedQuery();
          return;
        }
        this.filterHits.set(hits);
        this.filterOpen.set(true);
        this.appliedStudentId.set(undefined);
        this.runAppliedQuery();
      });
  }

  pickFilterStudent(row: BillingStudentSearchDto): void {
    this.filterStudent.set(row);
    this.filterQuery.set(row.fullName);
    this.filterHits.set([]);
    this.filterOpen.set(false);
    this.studentNotFound.set(false);
    this.commitScopeFilters();
    this.appliedStudentId.set(row.id);
    this.runAppliedQuery();
  }

  clearFilterStudent(): void {
    this.filterStudent.set(null);
    this.filterQuery.set('');
    this.filterHits.set([]);
    this.filterOpen.set(false);
    this.studentNotFound.set(false);
  }

  clearFilters(): void {
    this.filterStudent.set(null);
    this.filterQuery.set('');
    this.filterHits.set([]);
    this.filterOpen.set(false);
    this.studentNotFound.set(false);
    this.academicYearId.set(null);
    this.educationStageId.set(null);
    this.lessonId.set(null);
    this.groupId.set(null);
    this.sessionId.set(null);
    this.stages.set([]);
    this.lessons.set([]);
    this.groups.set([]);
    this.sessions.set([]);
    this.commitScopeFilters();
    this.appliedStudentId.set(undefined);
    this.runAppliedQuery();
  }

  onCollectSearch(value: string): void {
    this.collectQuery.set(value);
    this.collectSearchError.set(null);
    this.collectHits.set([]);
    this.collectOpenList.set(false);
    const picked = this.collectStudent();
    if (picked && value.trim() !== picked.fullName) {
      this.collectStudent.set(null);
      this.collectOutstanding.set(null);
      this.collectLessonId.set(null);
      this.selectedChargeIds.set(new Set());
    }
  }

  findCollectStudent(): void {
    const term = this.collectQuery().trim();
    this.collectSearchError.set(null);
    this.collectHits.set([]);
    this.collectOpenList.set(false);
    if (!term) {
      this.collectSearchError.set('empty');
      return;
    }

    const picked = this.collectStudent();
    if (picked && this.studentMatchesQuery(picked, term)) {
      this.loadCollectOutstanding(picked.id);
      return;
    }

    this.collectSearching.set(true);
    this.billingApi
      .searchStudents(term)
      .pipe(
        catchError((err) => {
          this.collectSearching.set(false);
          this.error.set(this.apiError(err, 'Failed to search students.'));
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        this.collectSearching.set(false);
        const hits = this.preferExactStudent(rows ?? [], term);
        if (hits.length === 1) {
          this.pickCollectStudent(hits[0]);
          return;
        }
        if (hits.length === 0) {
          this.collectStudent.set(null);
          this.collectOutstanding.set(null);
          this.collectLessonId.set(null);
          this.selectedChargeIds.set(new Set());
          this.collectSearchError.set('none');
          return;
        }
        this.collectHits.set(hits);
        this.collectOpenList.set(true);
      });
  }

  openCollect(): void {
    this.success.set(null);
    this.collectSearchError.set(null);
    this.collectSearching.set(false);
    this.collectOpen.set(true);
    this.collectStudent.set(null);
    this.collectOutstanding.set(null);
    this.collectLessonId.set(null);
    this.selectedChargeIds.set(new Set());
    this.collectQuery.set('');
    this.collectHits.set([]);
    this.collectOpenList.set(false);
    this.collectForm.reset({ amount: 0, method: PaymentMethod.Cash, note: '' });
  }

  closeCollect(): void {
    this.collectOpen.set(false);
    this.collectOutstanding.set(null);
    this.clearCollectQueryParams();
  }

  pickCollectStudent(row: BillingStudentSearchDto): void {
    this.collectStudent.set(row);
    this.collectQuery.set(row.fullName);
    this.collectHits.set([]);
    this.collectOpenList.set(false);
    this.collectSearchError.set(null);
    this.loadCollectOutstanding(row.id);
  }

  selectCollectLesson(lesson: StudentOutstandingLessonDto): void {
    this.collectLessonId.set(lesson.lessonId);
    this.selectedChargeIds.set(new Set(lesson.charges.map((c) => c.id)));
    this.collectForm.patchValue({ amount: Math.max(lesson.remaining ?? 0, 0) });
  }

  toggleCharge(chargeId: number): void {
    const next = new Set(this.selectedChargeIds());
    if (next.has(chargeId)) next.delete(chargeId);
    else next.add(chargeId);
    this.selectedChargeIds.set(next);
    this.collectForm.patchValue({ amount: Math.max(this.selectedRemaining(), 0) });
  }

  isChargeSelected(chargeId: number): boolean {
    return this.selectedChargeIds().has(chargeId);
  }

  fillMaxAmount(): void {
    this.collectForm.patchValue({ amount: Math.max(this.selectedRemaining(), 0) });
  }

  submitCollect(): void {
    const studentId = this.collectStudent()?.id;
    const lessonId = this.collectLessonId();
    if (!studentId || !lessonId || this.collectForm.invalid) {
      this.collectForm.markAllAsTouched();
      return;
    }
    const chargeIds = [...this.selectedChargeIds()];
    if (!chargeIds.length) {
      this.error.set('Select at least one open charge.');
      return;
    }
    const max = this.selectedRemaining();
    const value = this.collectForm.getRawValue();
    if (value.amount > max + 0.0001) {
      this.collectForm.patchValue({ amount: max });
      return;
    }

    this.collecting.set(true);
    this.error.set(null);
    this.billingApi
      .recordPayment(
        new RecordTeacherPaymentRequest({
          studentId,
          lessonId,
          amount: value.amount,
          method: value.method,
          note: value.note.trim() || undefined,
          chargeIds,
        }),
      )
      .pipe(
        concatMap(() => this.billingApi.getSummary(...this.summaryArgs())),
        tap((summary) => {
          this.summary.set(summary);
          this.success.set('paymentRecorded');
          this.collecting.set(false);
        }),
        concatMap(() => (this.section() ? this.listRequest() : of(null))),
        tap((list) => this.applyList(list)),
        concatMap(() => {
          const student = this.collectStudent();
          return student ? this.billingApi.getStudentOutstanding(student.id) : of(null);
        }),
        catchError((err) => {
          this.collecting.set(false);
          this.error.set(this.apiError(err, 'Failed to record payment.'));
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((sheet) => {
        if (sheet) this.applyCollectSheet(sheet);
      });
  }

  openTransaction(row: LedgerTransactionDto): void {
    this.openDetail(row.kind === 'Payment' ? 'Payment' : 'Charge', row.id);
  }

  openCharge(row: LedgerChargeRowDto): void {
    this.openDetail('Charge', row.id);
  }

  openPayment(row: LedgerPaymentRowDto): void {
    this.openDetail('Payment', row.id);
  }

  closeDetail(): void {
    this.detailOpen.set(false);
    this.chargeDetail.set(null);
    this.paymentDetail.set(null);
  }

  downloadPayment(id: number, receiptNumber?: number): void {
    this.downloading.set(true);
    this.billingApi
      .downloadReceipt(id)
      .pipe(
        catchError((err) => {
          this.downloading.set(false);
          this.error.set(this.apiError(err, 'Failed to download receipt.'));
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((file) => {
        this.downloading.set(false);
        this.saveBlob(file.data, file.fileName || `receipt-${receiptNumber ?? id}.pdf`);
      });
  }

  chargeTypeKey(type?: string | null): string {
    switch (type) {
      case 'MonthlyCycle':
        return 'billing.monthlyCycle';
      case 'Makeup':
        return 'billing.makeupCharge';
      case 'Adjustment':
        return 'billing.adjustmentCharge';
      default:
        return 'billing.sessionCharge';
    }
  }

  statusKey(status?: string | null): string {
    switch (status) {
      case 'Paid':
        return 'billing.statusPaid';
      case 'Partial':
        return 'billing.statusPartial';
      case 'Open':
        return 'billing.statusOpen';
      case 'Deferred':
        return 'billing.statusDeferred';
      default:
        return 'billing.statusNone';
    }
  }

  methodKey(method?: string | null): string {
    switch (method) {
      case 'VodafoneCash':
        return 'billing.methodVodafone';
      case 'InstaPay':
        return 'billing.methodInstaPay';
      case 'Other':
        return 'billing.methodOther';
      default:
        return 'billing.methodCash';
    }
  }

  statusTone(status?: string | null): string {
    switch (status) {
      case 'Paid':
        return 'is-paid';
      case 'Partial':
        return 'is-partial';
      case 'Deferred':
        return 'is-deferred';
      default:
        return 'is-open';
    }
  }

  sessionLabel(session: LedgerFilterSessionDto): string {
    const date = session.sessionDate
      ? new Date(session.sessionDate).toISOString().slice(0, 10)
      : '';
    const topic = session.topic ? ` · ${session.topic}` : '';
    const makeup = session.isMakeup ? ' *' : '';
    return `${date} ${session.startTime || ''}${topic}${makeup}`.trim();
  }

  chargeContext(row: LedgerChargeRowDto): string {
    return [row.academicYearName, row.educationStageName, row.groupName].filter(Boolean).join(' · ');
  }

  chargeSession(row: LedgerChargeRowDto): string {
    if (!row.sessionDate && !row.sessionTopic) return '';
    const date = row.sessionDate ? new Date(row.sessionDate).toISOString().slice(0, 10) : '';
    return [date, row.sessionStartTime, row.sessionTopic].filter(Boolean).join(' · ');
  }

  private boot(studentId: number | null): void {
    this.loadSummary();
    this.billingApi
      .getFilterAcademicYears()
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load ledger.'));
          return of([] as LedgerFilterOptionDto[]);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((years) => {
        this.academicYears.set(years ?? []);
      });

    if (this.academicYearId()) this.loadStages();
    if (this.educationStageId()) this.loadLessonsFilter();
    if (this.lessonId()) this.loadGroups();
    if (this.groupId()) this.loadSessions();
    if (studentId) this.openCollectForStudent(studentId);
  }

  private loadStages(): void {
    const academicYearId = this.academicYearId();
    if (!academicYearId) return;
    this.loadingStages.set(true);
    this.billingApi
      .getFilterStages(academicYearId)
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load stages.'));
          return of([] as LedgerFilterOptionDto[]);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        this.stages.set(rows ?? []);
        this.loadingStages.set(false);
      });
  }

  private loadLessonsFilter(): void {
    if (!this.educationStageId()) return;
    this.loadingLessons.set(true);
    this.billingApi
      .getFilterLessons(this.academicYearId() ?? undefined, this.educationStageId() ?? undefined)
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load lessons.'));
          return of([] as LedgerFilterOptionDto[]);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        this.lessons.set(rows ?? []);
        this.loadingLessons.set(false);
      });
  }

  private loadGroups(): void {
    const lessonId = this.lessonId();
    if (!lessonId) return;
    this.loadingGroups.set(true);
    this.billingApi
      .getFilterGroups(lessonId)
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load groups.'));
          return of([] as LedgerFilterOptionDto[]);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        this.groups.set(rows ?? []);
        this.loadingGroups.set(false);
      });
  }

  private loadSessions(): void {
    const groupId = this.groupId();
    if (!groupId) return;
    this.loadingSessions.set(true);
    this.billingApi
      .getFilterSessions(groupId)
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load sessions.'));
          return of([] as LedgerFilterSessionDto[]);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        this.sessions.set(rows ?? []);
        this.loadingSessions.set(false);
      });
  }

  private reloadOpenSection(): void {
    if (!this.section()) return;
    this.loadList();
  }

  private commitScopeFilters(): void {
    this.appliedAcademicYearId.set(this.academicYearId() ?? undefined);
    this.appliedEducationStageId.set(this.educationStageId() ?? undefined);
    this.appliedLessonId.set(this.lessonId() ?? undefined);
    this.appliedGroupId.set(this.groupId() ?? undefined);
    this.appliedSessionId.set(this.sessionId() ?? undefined);
  }

  private runAppliedQuery(): void {
    this.page.set(1);
    this.loadSummary();
    this.reloadOpenSection();
  }

  private studentMatchesQuery(row: BillingStudentSearchDto, query: string): boolean {
    const q = this.normalizeSearch(query);
    if (!q) return false;
    return (
      this.normalizeSearch(row.fullName) === q ||
      this.normalizeSearch(row.studentCode) === q ||
      this.normalizeSearch(row.phoneNumber) === q
    );
  }

  private preferExactStudent(rows: BillingStudentSearchDto[], query: string): BillingStudentSearchDto[] {
    const q = this.normalizeSearch(query);
    const exact = rows.filter(
      (row) => this.normalizeSearch(row.studentCode) === q || this.normalizeSearch(row.phoneNumber) === q,
    );
    return exact.length ? exact : rows;
  }

  private normalizeSearch(value?: string | null): string {
    return (value ?? '').trim().toLowerCase().replace(/\s+/g, '');
  }

  private loadSummary(): void {
    this.billingApi
      .getSummary(...this.summaryArgs())
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load ledger.'));
          return of(null);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((summary) => {
        if (summary) this.summary.set(summary);
      });
  }

  private summaryArgs(): [
    number | undefined,
    number | undefined,
    number | undefined,
    number | undefined,
    number | undefined,
    number | undefined,
  ] {
    return [
      this.appliedStudentId(),
      this.appliedAcademicYearId(),
      this.appliedEducationStageId(),
      this.appliedLessonId(),
      this.appliedGroupId(),
      this.appliedSessionId(),
    ];
  }

  private loadList(): void {
    if (!this.section()) return;
    this.loadingList.set(true);
    this.list$.next();
  }

  private listRequest(): Observable<{ items?: unknown[]; totalCount?: number } | null> {
    const [studentId, academicYearId, educationStageId, lessonId, groupId, sessionId] = this.summaryArgs();
    const page = this.page();
    const section = this.section();
    const request$ = (
      section === 'outstanding'
        ? this.billingApi.getOutstanding(
            studentId,
            academicYearId,
            educationStageId,
            lessonId,
            groupId,
            sessionId,
            undefined,
            undefined,
            undefined,
            page,
            PAGE_SIZE,
          )
        : section === 'charges'
          ? this.billingApi.getCharges(
              studentId,
              academicYearId,
              educationStageId,
              lessonId,
              groupId,
              sessionId,
              undefined,
              undefined,
              undefined,
              undefined,
              page,
              PAGE_SIZE,
            )
          : section === 'payments'
            ? this.billingApi.getPayments(
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId,
                undefined,
                undefined,
                page,
                PAGE_SIZE,
              )
            : this.billingApi.getTransactions(
                studentId,
                academicYearId,
                educationStageId,
                lessonId,
                groupId,
                sessionId,
                undefined,
                undefined,
                undefined,
                undefined,
                page,
                PAGE_SIZE,
              )
    ) as Observable<{ items?: unknown[]; totalCount?: number }>;

    return request$.pipe(
      catchError((err) => {
        this.error.set(this.apiError(err, 'Failed to load ledger.'));
        this.loadingList.set(false);
        return of(null);
      }),
    );
  }

  private applyList(data: { items?: unknown[]; totalCount?: number } | null): void {
    this.loadingList.set(false);
    if (!data) {
      this.transactions.set([]);
      this.charges.set([]);
      this.payments.set([]);
      this.totalCount.set(0);
      return;
    }

    this.totalCount.set(data.totalCount ?? 0);
    switch (this.section()) {
      case 'payments':
        this.payments.set((data.items as LedgerPaymentRowDto[]) ?? []);
        this.transactions.set([]);
        this.charges.set([]);
        break;
      case 'transactions':
        this.transactions.set((data.items as LedgerTransactionDto[]) ?? []);
        this.charges.set([]);
        this.payments.set([]);
        break;
      default:
        this.charges.set((data.items as LedgerChargeRowDto[]) ?? []);
        this.transactions.set([]);
        this.payments.set([]);
        break;
    }
  }

  private openCollectForStudent(studentId: number): void {
    this.collectOpen.set(true);
    this.collectStudent.set(new BillingStudentSearchDto({ id: studentId, fullName: '' }));
    this.loadCollectOutstanding(studentId);
  }

  private loadCollectOutstanding(studentId: number): void {
    this.loadingOutstanding.set(true);
    this.billingApi
      .getStudentOutstanding(studentId)
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load outstanding.'));
          this.loadingOutstanding.set(false);
          return of(null);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((sheet) => {
        this.loadingOutstanding.set(false);
        if (sheet) this.applyCollectSheet(sheet);
      });
  }

  private applyCollectSheet(sheet: StudentOutstandingDto): void {
    this.collectOutstanding.set(sheet);
    this.collectStudent.set(
      new BillingStudentSearchDto({
        id: sheet.studentId,
        fullName: sheet.studentName,
        studentCode: sheet.studentCode,
      }),
    );
    this.collectQuery.set(sheet.studentName);
    const match = sheet.lessons.find((l) => l.lessonId === this.lessonId()) ?? sheet.lessons[0];
    if (match) this.selectCollectLesson(match);
  }

  private openDetail(kind: 'Charge' | 'Payment', id: number): void {
    this.detailOpen.set(true);
    this.detailKind.set(kind);
    this.loadingDetail.set(true);
    this.chargeDetail.set(null);
    this.paymentDetail.set(null);
    const call$ = (
      kind === 'Charge' ? this.billingApi.getCharge(id) : this.billingApi.getPayment(id)
    ) as Observable<LedgerChargeDetailDto | LedgerPaymentDetailDto | null>;
    call$
      .pipe(
        catchError((err) => {
          this.error.set(this.apiError(err, 'Failed to load details.'));
          this.loadingDetail.set(false);
          return of(null);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        this.loadingDetail.set(false);
        if (!data) return;
        if (kind === 'Charge') this.chargeDetail.set(data as LedgerChargeDetailDto);
        else this.paymentDetail.set(data as LedgerPaymentDetailDto);
      });
  }

  private clearCollectQueryParams(): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { studentId: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  private saveBlob(data: Blob, fileName: string): void {
    const url = URL.createObjectURL(data);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  }

  private apiError(err: unknown, fallback: string): string {
    const e = err as {
      result?: { detail?: string };
      error?: { detail?: string; title?: string };
      message?: string;
    };
    return e?.result?.detail || e?.error?.detail || e?.error?.title || e?.message || fallback;
  }
}
