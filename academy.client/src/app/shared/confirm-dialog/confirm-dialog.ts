import { Component, HostListener, inject } from '@angular/core';
import { ConfirmDialogService } from '../../core/ui/confirm-dialog.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialogComponent {
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly state = this.confirmDialog.state;
  readonly isOpen = this.confirmDialog.isOpen;

  onConfirm(event: MouseEvent): void {
    event.stopPropagation();
    this.confirmDialog.confirm();
  }

  onCancel(event?: MouseEvent): void {
    event?.stopPropagation();
    this.confirmDialog.cancel();
  }

  onBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.confirmDialog.cancel();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen()) {
      this.confirmDialog.cancel();
    }
  }
}
