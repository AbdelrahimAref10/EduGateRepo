import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ParentApi, ParentChildDto } from '../../core/api/parent-api.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { ConfirmDialogService } from '../../core/ui/confirm-dialog.service';

@Component({
  selector: 'app-parent-children',
  standalone: true,
  imports: [TranslatePipe, DatePipe, FormsModule],
  templateUrl: './parent-children.html',
})
export class ParentChildrenComponent implements OnInit {
  private readonly api = inject(ParentApi);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly loading = signal(false);
  readonly linking = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly children = signal<ParentChildDto[]>([]);
  studentCode = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getChildren().subscribe({
      next: (items) => {
        this.children.set(items);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to load children.');
      },
    });
  }

  link(): void {
    const code = this.studentCode.trim();
    if (!code) return;
    this.linking.set(true);
    this.error.set(null);
    this.success.set(null);
    this.api.linkChild(code).subscribe({
      next: () => {
        this.linking.set(false);
        this.studentCode = '';
        this.success.set('linked');
        this.load();
      },
      error: (err) => {
        this.linking.set(false);
        this.error.set(err?.error?.detail || err?.message || 'Failed to link child.');
      },
    });
  }

  async unlink(child: ParentChildDto): Promise<void> {
    const ok = await this.confirmDialog.ask({
      titleKey: 'parent.unlinkTitle',
      messageKey: 'parent.unlinkBody',
      tone: 'danger',
    });
    if (!ok) return;

    this.api.unlinkChild(child.childStudentId).subscribe({
      next: () => this.load(),
      error: (err) => {
        this.error.set(err?.error?.detail || err?.message || 'Failed to unlink.');
      },
    });
  }
}
