import { Injectable, computed, signal } from '@angular/core';

export type ConfirmTone = 'primary' | 'danger' | 'warning';

export interface ConfirmDialogRequest {
  titleKey?: string;
  messageKey: string;
  confirmKey?: string;
  cancelKey?: string;
  tone?: ConfirmTone;
}

export interface ConfirmDialogState {
  titleKey: string;
  messageKey: string;
  confirmKey: string;
  cancelKey: string;
  tone: ConfirmTone;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly stateSignal = signal<ConfirmDialogState | null>(null);
  private resolver: ((value: boolean) => void) | null = null;

  readonly state = this.stateSignal.asReadonly();
  readonly isOpen = computed(() => this.stateSignal() !== null);

  ask(request: ConfirmDialogRequest): Promise<boolean> {
    if (this.resolver) {
      this.resolver(false);
      this.resolver = null;
    }

    this.stateSignal.set({
      titleKey: request.titleKey ?? 'common.confirmTitle',
      messageKey: request.messageKey,
      confirmKey: request.confirmKey ?? 'common.confirmAction',
      cancelKey: request.cancelKey ?? 'common.cancel',
      tone: request.tone ?? 'primary',
    });

    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
    });
  }

  confirm(): void {
    this.finish(true);
  }

  cancel(): void {
    this.finish(false);
  }

  private finish(value: boolean): void {
    const resolve = this.resolver;
    this.resolver = null;
    this.stateSignal.set(null);
    resolve?.(value);
  }
}
