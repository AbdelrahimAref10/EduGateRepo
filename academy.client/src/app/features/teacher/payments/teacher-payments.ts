import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  BillingCatalogDto,
  BillingClient,
  BillingEducationTypeNodeDto,
  BillingLessonSummaryDto,
  BillingStageNodeDto,
  ChargeDto,
  GroupBillingDto,
  LedgerStudentRowDto,
  LessonBillingDetailDto,
  PaymentDto,
  PaymentMethod,
  RecordPaymentRequest,
  StudentLessonLedgerDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

export type PayStep = 'type' | 'stage' | 'lesson' | 'students';

@Component({
  selector: 'app-teacher-payments',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    ReactiveFormsModule,
    TranslatePipe,
    PageLoaderComponent,
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

  readonly loadingCatalog = signal(true);
  readonly ready = signal(false);
  readonly loadingDetail = signal(false);
  readonly loadingLedger = signal(false);
  readonly collecting = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly catalog = signal<BillingCatalogDto | null>(null);
  readonly step = signal<PayStep>('type');
  readonly selectedTypeId = signal<number | null>(null);
  readonly selectedStageId = signal<number | null>(null);
  readonly debtOnly = signal(true);

  readonly selectedLessonId = signal<number | null>(null);
  readonly selectedLessonSummary = signal<BillingLessonSummaryDto | null>(null);
  readonly detail = signal<LessonBillingDetailDto | null>(null);
  readonly selectedGroupId = signal<number | null>(null);

  readonly collectOpen = signal(false);
  readonly collectLessonId = signal<number | null>(null);
  readonly collectStudentId = signal<number | null>(null);
  readonly studentLedger = signal<StudentLessonLedgerDto | null>(null);
  readonly selectedChargeIds = signal<Set<number>>(new Set());

  readonly paymentMethods = [
    { value: PaymentMethod.Cash, key: 'billing.methodCash' },
    { value: PaymentMethod.VodafoneCash, key: 'billing.methodVodafone' },
    { value: PaymentMethod.InstaPay, key: 'billing.methodInstaPay' },
    { value: PaymentMethod.Other, key: 'billing.methodOther' },
  ];

  readonly collectForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: [PaymentMethod.Cash, Validators.required],
    note: [''],
  });

  readonly educationTypes = computed(() => this.catalog()?.educationTypes ?? []);

  readonly selectedType = computed(() => {
    const id = this.selectedTypeId();
    return this.educationTypes().find((t) => t.educationTypeId === id) ?? null;
  });

  readonly stagesForType = computed(() => this.selectedType()?.stages ?? []);

  readonly selectedStage = computed(() => {
    const id = this.selectedStageId();
    return this.stagesForType().find((s) => s.educationStageId === id) ?? null;
  });

  readonly lessonsForStage = computed(() => {
    const lessons = this.selectedStage()?.lessons ?? [];
    if (!this.debtOnly()) return lessons;
    return lessons.filter((l) => (l.outstandingAmount ?? 0) > 0);
  });

  readonly groups = computed(() => this.detail()?.groups ?? []);

  readonly activeGroup = computed(() => {
    const id = this.selectedGroupId();
    const list = this.groups();
    return list.find((g) => g.groupId === id) ?? list[0] ?? null;
  });

  readonly stepIndex = computed(() => {
    switch (this.step()) {
      case 'type':
        return 1;
      case 'stage':
        return 2;
      case 'lesson':
        return 3;
      case 'students':
        return 4;
    }
  });

  readonly promptKey = computed(() => {
    switch (this.step()) {
      case 'type':
        return 'teacherPayments.askType';
      case 'stage':
        return 'teacherPayments.askStage';
      case 'lesson':
        return 'teacherPayments.askLesson';
      case 'students':
        return 'teacherPayments.askStudent';
    }
  });

  readonly openCharges = computed(() => {
    const led = this.studentLedger();
    if (!led?.charges?.length) return [] as ChargeDto[];
    return led.charges.filter(
      (c) => c.status !== 'Deferred' && c.status !== 'Paid' && (c.remaining ?? 0) > 0,
    );
  });

  readonly selectedRemaining = computed(() => {
    const ids = this.selectedChargeIds();
    return this.openCharges()
      .filter((c) => ids.has(c.id))
      .reduce((sum, c) => sum + Number(c.remaining || 0), 0);
  });

  ngOnInit(): void {
    const qLesson = Number(this.route.snapshot.queryParamMap.get('lessonId') || 0);
    const qStudent = Number(this.route.snapshot.queryParamMap.get('studentId') || 0);
    this.loadCatalog(qLesson > 0 ? qLesson : null, qStudent > 0 ? qStudent : null);
  }

  loadCatalog(
    autoSelectLessonId: number | null = null,
    autoCollectStudentId: number | null = null,
  ): void {
    this.loadingCatalog.set(true);
    this.error.set(null);
    this.billingApi.getBillingCatalog().subscribe({
      next: (data) => {
        this.catalog.set(data);
        this.loadingCatalog.set(false);
        this.ready.set(true);

        if (autoSelectLessonId) {
          this.applySelectionPath(data, autoSelectLessonId);
          const summary = this.findLessonSummary(autoSelectLessonId, data);
          if (summary) this.selectedLessonSummary.set(summary);
          this.loadLessonDetail(autoSelectLessonId, autoCollectStudentId, true);
          return;
        }

        // Fresh entry: start at type — don't dump the teacher into a deep lesson.
        this.selectedTypeId.set(null);
        this.selectedStageId.set(null);
        this.selectedLessonId.set(null);
        this.selectedLessonSummary.set(null);
        this.detail.set(null);
        this.step.set('type');

        const types = data.educationTypes ?? [];
        if (types.length === 1) {
          this.enterType(types[0].educationTypeId, true);
        }
      },
      error: (err) => {
        this.loadingCatalog.set(false);
        this.ready.set(true);
        this.error.set(this.apiError(err, 'Failed to load billing catalog.'));
      },
    });
  }

  enterType(typeId: number, autoAdvance = true): void {
    this.selectedTypeId.set(typeId);
    this.selectedStageId.set(null);
    this.clearLesson();
    this.step.set('stage');

    const type = this.educationTypes().find((t) => t.educationTypeId === typeId);
    const stages = type?.stages ?? [];
    if (autoAdvance && stages.length === 1) {
      this.enterStage(stages[0].educationStageId, true);
    }
  }

  enterStage(stageId: number, autoAdvance = false): void {
    this.selectedStageId.set(stageId);
    this.clearLesson();
    this.step.set('lesson');

    const stage = this.stagesForType().find((s) => s.educationStageId === stageId);
    const lessons = stage?.lessons ?? [];
    const owed = lessons.filter((l) => (l.outstandingAmount ?? 0) > 0);
    if (autoAdvance && owed.length === 1) {
      this.enterLesson(owed[0].lessonId);
    } else if (autoAdvance && lessons.length === 1) {
      this.enterLesson(lessons[0].lessonId);
    }
  }

  enterLesson(lessonId: number, autoCollectStudentId: number | null = null): void {
    this.loadLessonDetail(lessonId, autoCollectStudentId, true);
  }

  goToStep(target: PayStep): void {
    if (target === 'type') {
      this.selectedTypeId.set(null);
      this.selectedStageId.set(null);
      this.clearLesson();
      this.step.set('type');
      return;
    }
    if (target === 'stage') {
      if (!this.selectedTypeId()) return;
      this.selectedStageId.set(null);
      this.clearLesson();
      this.step.set('stage');
      return;
    }
    if (target === 'lesson') {
      if (!this.selectedStageId()) return;
      this.clearLesson();
      this.step.set('lesson');
      return;
    }
    if (this.selectedLessonId() && this.detail()) {
      this.step.set('students');
    }
  }

  goBack(): void {
    switch (this.step()) {
      case 'students':
        this.goToStep('lesson');
        break;
      case 'lesson':
        this.goToStep('stage');
        break;
      case 'stage':
        this.goToStep('type');
        break;
      default:
        break;
    }
  }

  toggleDebtOnly(): void {
    this.debtOnly.update((v) => !v);
  }

  selectGroup(groupId: number): void {
    this.selectedGroupId.set(groupId);
  }

  openCollect(lessonId: number, row: LedgerStudentRowDto): void {
    this.collectLessonId.set(lessonId);
    this.collectStudentId.set(row.studentId);
    this.studentLedger.set(null);
    this.selectedChargeIds.set(new Set());
    this.collectOpen.set(true);
    this.success.set(null);
    this.error.set(null);
    this.loadingLedger.set(true);

    this.collectForm.reset({
      amount: Math.max(Number(row.outstandingAmount || 0), 0),
      method: PaymentMethod.Cash,
      note: '',
    });

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { lessonId, studentId: row.studentId },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });

    this.billingApi.getStudentLedger(lessonId, row.studentId).subscribe({
      next: (ledger) => {
        this.studentLedger.set(ledger);
        this.loadingLedger.set(false);
        const open = (ledger.charges ?? []).filter(
          (c) => c.status !== 'Deferred' && c.status !== 'Paid' && (c.remaining ?? 0) > 0,
        );
        this.selectedChargeIds.set(new Set(open.map((c) => c.id)));
        const max = open.reduce((sum, c) => sum + Number(c.remaining || 0), 0);
        this.collectForm.patchValue({ amount: Math.max(max, 0) });
      },
      error: (err) => {
        this.loadingLedger.set(false);
        this.error.set(this.apiError(err, 'Failed to load student ledger.'));
      },
    });
  }

  closeCollect(): void {
    this.collectOpen.set(false);
    this.collectStudentId.set(null);
    this.studentLedger.set(null);
    this.selectedChargeIds.set(new Set());
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { studentId: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  toggleCharge(chargeId: number): void {
    this.selectedChargeIds.update((set) => {
      const next = new Set(set);
      if (next.has(chargeId)) next.delete(chargeId);
      else next.add(chargeId);
      return next;
    });
    const max = this.selectedRemaining();
    const current = Number(this.collectForm.controls.amount.value || 0);
    if (current > max) {
      this.collectForm.patchValue({ amount: Math.max(max, 0) });
    } else if (current <= 0 && max > 0) {
      this.collectForm.patchValue({ amount: max });
    }
  }

  isChargeSelected(chargeId: number): boolean {
    return this.selectedChargeIds().has(chargeId);
  }

  fillMaxAmount(): void {
    this.collectForm.patchValue({ amount: Math.max(this.selectedRemaining(), 0) });
  }

  submitCollect(): void {
    const lessonId = this.collectLessonId();
    const studentId = this.collectStudentId();
    if (!lessonId || !studentId || this.collectForm.invalid) {
      this.collectForm.markAllAsTouched();
      return;
    }

    const chargeIds = [...this.selectedChargeIds()];
    if (chargeIds.length === 0) {
      this.error.set('Select at least one open charge.');
      return;
    }

    const max = this.selectedRemaining();
    const value = this.collectForm.getRawValue();
    if (value.amount > max + 0.0001) {
      this.error.set(`Amount cannot exceed remaining (${max}).`);
      this.collectForm.patchValue({ amount: max });
      return;
    }

    this.collecting.set(true);
    this.error.set(null);

    this.billingApi
      .recordPayment(
        lessonId,
        new RecordPaymentRequest({
          studentId,
          amount: value.amount,
          method: value.method,
          note: value.note.trim() || undefined,
          chargeIds,
        }),
      )
      .subscribe({
        next: () => {
          this.collecting.set(false);
          this.success.set('paymentRecorded');
          this.refreshAfterPayment(lessonId);
          this.billingApi.getStudentLedger(lessonId, studentId).subscribe({
            next: (ledger) => {
              this.studentLedger.set(ledger);
              const open = (ledger.charges ?? []).filter(
                (c) => c.status !== 'Deferred' && c.status !== 'Paid' && (c.remaining ?? 0) > 0,
              );
              this.selectedChargeIds.set(new Set(open.map((c) => c.id)));
              const nextMax = open.reduce((sum, c) => sum + Number(c.remaining || 0), 0);
              this.collectForm.patchValue({
                amount: Math.max(nextMax, 0),
                method: PaymentMethod.Cash,
                note: '',
              });
            },
            error: () => this.closeCollect(),
          });
        },
        error: (err) => {
          this.collecting.set(false);
          this.error.set(this.apiError(err, 'Failed to record payment.'));
        },
      });
  }

  downloadReceipt(payment: PaymentDto): void {
    this.billingApi.downloadReceipt(payment.id).subscribe({
      next: (file) =>
        this.saveBlob(file.data, file.fileName || `receipt-${payment.receiptNumber}.pdf`),
      error: (err) => this.error.set(this.apiError(err, 'Failed to download receipt.')),
    });
  }

  billingTypeKey(type?: string | null): string {
    return type === 'Monthly' ? 'billing.monthlyCycle' : 'billing.sessionCharge';
  }

  chargeTypeKey(type?: string | null): string {
    switch (type) {
      case 'MonthlyCycle':
        return 'billing.monthlyCycle';
      case 'Makeup':
        return 'billing.makeupCharge';
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
      default:
        return 'billing.statusNone';
    }
  }

  studentTone(row: LedgerStudentRowDto): string {
    if ((row.outstandingAmount ?? 0) <= 0) return 'pay-pill is-paid';
    if ((row.lastPaymentAmount ?? 0) > 0) return 'pay-pill is-partial';
    return 'pay-pill is-open';
  }

  studentStatusKey(row: LedgerStudentRowDto): string {
    if ((row.outstandingAmount ?? 0) <= 0) return 'billing.statusPaid';
    if ((row.lastPaymentAmount ?? 0) > 0) return 'billing.statusPartial';
    return 'billing.statusOpen';
  }

  trackType(_: number, t: BillingEducationTypeNodeDto): number {
    return t.educationTypeId;
  }

  trackStage(_: number, s: BillingStageNodeDto): number {
    return s.educationStageId;
  }

  trackLesson(_: number, l: BillingLessonSummaryDto): number {
    return l.lessonId;
  }

  trackGroup(_: number, g: GroupBillingDto): number {
    return g.groupId;
  }

  private loadLessonDetail(
    lessonId: number,
    autoCollectStudentId: number | null,
    goStudents: boolean,
  ): void {
    const summary = this.findLessonSummary(lessonId);
    if (summary) {
      this.selectedLessonSummary.set(summary);
      this.ensurePathForLesson(summary);
    }

    if (this.selectedLessonId() === lessonId && this.detail()?.lessonId === lessonId) {
      if (goStudents) this.step.set('students');
      if (autoCollectStudentId) {
        const row = this.findStudentRow(autoCollectStudentId);
        if (row) this.openCollect(lessonId, row);
      }
      return;
    }

    this.selectedLessonId.set(lessonId);
    this.detail.set(null);
    this.selectedGroupId.set(null);
    this.loadingDetail.set(true);
    this.error.set(null);
    if (goStudents) this.step.set('students');

    this.billingApi.getLessonBillingDetail(lessonId).subscribe({
      next: (data) => {
        this.detail.set(data);
        this.loadingDetail.set(false);
        const preferred =
          (data.groups ?? []).find((g) => g.outstandingAmount > 0)?.groupId ??
          data.groups?.[0]?.groupId ??
          null;
        this.selectedGroupId.set(preferred);

        if (autoCollectStudentId) {
          const row = this.findStudentRow(autoCollectStudentId, data);
          if (row) this.openCollect(lessonId, row);
        }

        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { lessonId },
          queryParamsHandling: 'merge',
          replaceUrl: true,
        });
      },
      error: (err) => {
        this.loadingDetail.set(false);
        this.error.set(this.apiError(err, 'Failed to load lesson billing.'));
      },
    });
  }

  private clearLesson(): void {
    this.selectedLessonId.set(null);
    this.selectedLessonSummary.set(null);
    this.detail.set(null);
    this.selectedGroupId.set(null);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { lessonId: null, studentId: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  private applySelectionPath(data: BillingCatalogDto, lessonId: number): void {
    for (const type of data.educationTypes ?? []) {
      for (const stage of type.stages ?? []) {
        if ((stage.lessons ?? []).some((l) => l.lessonId === lessonId)) {
          this.selectedTypeId.set(type.educationTypeId);
          this.selectedStageId.set(stage.educationStageId);
          return;
        }
      }
    }
  }

  private ensurePathForLesson(summary: BillingLessonSummaryDto): void {
    const data = this.catalog();
    if (!data) return;
    for (const type of data.educationTypes ?? []) {
      for (const stage of type.stages ?? []) {
        if ((stage.lessons ?? []).some((l) => l.lessonId === summary.lessonId)) {
          this.selectedTypeId.set(type.educationTypeId);
          this.selectedStageId.set(stage.educationStageId);
          return;
        }
      }
    }
  }

  private refreshAfterPayment(lessonId: number): void {
    // Keep the teacher on the same lesson after settle.
    this.billingApi.getBillingCatalog().subscribe({
      next: (data) => {
        this.catalog.set(data);
        this.applySelectionPath(data, lessonId);
        const summary = this.findLessonSummary(lessonId, data);
        if (summary) this.selectedLessonSummary.set(summary);
        this.billingApi.getLessonBillingDetail(lessonId).subscribe({
          next: (detail) => {
            this.detail.set(detail);
            const preferred =
              (detail.groups ?? []).find((g) => g.groupId === this.selectedGroupId()) ??
              (detail.groups ?? []).find((g) => g.outstandingAmount > 0) ??
              detail.groups?.[0] ??
              null;
            this.selectedGroupId.set(preferred?.groupId ?? null);
            this.step.set('students');
          },
        });
      },
    });
  }

  private findLessonSummary(
    lessonId: number,
    data: BillingCatalogDto | null = this.catalog(),
  ): BillingLessonSummaryDto | null {
    for (const type of data?.educationTypes ?? []) {
      for (const stage of type.stages ?? []) {
        const hit = (stage.lessons ?? []).find((l) => l.lessonId === lessonId);
        if (hit) return hit;
      }
    }
    return null;
  }

  private findStudentRow(
    studentId: number,
    detail: LessonBillingDetailDto | null = this.detail(),
  ): LedgerStudentRowDto | null {
    for (const g of detail?.groups ?? []) {
      const hit = g.students?.find((s) => s.studentId === studentId);
      if (hit) return hit;
    }
    return null;
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
