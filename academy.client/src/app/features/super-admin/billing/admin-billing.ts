import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ChargeDto,
  PaymentDto,
  StudentLessonLedgerDto,
} from '../../../core/api/academy-api.generated';
import {
  AdminBillingService,
  AdminDebtRowDto,
} from '../../../core/api/admin-billing.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

@Component({
  selector: 'app-admin-billing',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    FormsModule,
    TranslatePipe,
    UserAvatarComponent,
    PageLoaderComponent,
  ],
  templateUrl: './admin-billing.html',
  styleUrl: './admin-billing.css',
})
export class AdminBillingComponent implements OnInit {
  private readonly api = inject(AdminBillingService);

  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly error = signal<string | null>(null);
  readonly debts = signal<AdminDebtRowDto[]>([]);
  readonly lessonFilter = signal('');

  readonly ledgerOpen = signal(false);
  readonly ledgerLoading = signal(false);
  readonly ledger = signal<StudentLessonLedgerDto | null>(null);
  readonly downloadingPaymentId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadDebts();
  }

  loadDebts(): void {
    this.loading.set(true);
    this.error.set(null);
    const raw = this.lessonFilter().trim();
    const lessonId = raw ? Number(raw) : null;
    this.api.getDebts(lessonId && lessonId > 0 ? lessonId : null).subscribe({
      next: (rows) => {
        this.debts.set(rows ?? []);
        this.loading.set(false);
        this.ready.set(true);
      },
      error: (err) => {
        this.loading.set(false);
        this.ready.set(true);
        this.error.set(this.apiError(err, 'Failed to load debts.'));
      },
    });
  }

  openLedger(row: AdminDebtRowDto): void {
    this.ledgerOpen.set(true);
    this.ledger.set(null);
    this.ledgerLoading.set(true);
    this.error.set(null);
    this.api.getStudentLedger(row.lessonId, row.studentId).subscribe({
      next: (data) => {
        this.ledger.set(data);
        this.ledgerLoading.set(false);
      },
      error: (err) => {
        this.ledgerLoading.set(false);
        this.error.set(this.apiError(err, 'Failed to load ledger.'));
      },
    });
  }

  closeLedger(): void {
    this.ledgerOpen.set(false);
    this.ledger.set(null);
  }

  downloadReceipt(payment: PaymentDto): void {
    if (!payment?.id) return;
    this.downloadingPaymentId.set(payment.id);
    this.api.downloadReceipt(payment.id).subscribe({
      next: (file) => {
        this.downloadingPaymentId.set(null);
        this.saveBlob(file.data, file.fileName || `receipt-${payment.receiptNumber}.pdf`);
      },
      error: (err) => {
        this.downloadingPaymentId.set(null);
        this.error.set(this.apiError(err, 'Failed to download receipt.'));
      },
    });
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

  billingTone(status?: string | null): string {
    switch (status) {
      case 'Paid':
        return 'billing-pill is-paid';
      case 'Partial':
        return 'billing-pill is-partial';
      case 'Open':
        return 'billing-pill is-open';
      default:
        return 'billing-pill is-none';
    }
  }

  methodKey(method?: string | null): string {
    switch (method) {
      case 'Cash':
      case '1':
        return 'billing.methodCash';
      case 'VodafoneCash':
      case '2':
        return 'billing.methodVodafone';
      case 'InstaPay':
      case '3':
        return 'billing.methodInstaPay';
      default:
        return 'billing.methodOther';
    }
  }

  chargeTypeKey(type?: string | null): string {
    switch (type) {
      case 'Session':
        return 'billing.sessionCharge';
      case 'MonthlyCycle':
        return 'billing.monthlyCycle';
      case 'Makeup':
        return 'billing.makeupCharge';
      default:
        return 'billing.type';
    }
  }

  chargeTone(charge: ChargeDto): string {
    return this.billingTone(charge.status);
  }

  private saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  private apiError(err: unknown, fallback: string): string {
    const e = err as { detail?: string; title?: string; result?: { detail?: string; title?: string } };
    return e?.detail || e?.title || e?.result?.detail || e?.result?.title || fallback;
  }
}
