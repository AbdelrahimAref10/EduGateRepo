import { Component, effect, inject, input, signal } from '@angular/core';
import { ImageService } from '../../core/images/image.service';

@Component({
  selector: 'app-user-avatar',
  standalone: true,
  template: `
    <img
      [src]="displaySrc()"
      [alt]="alt()"
      class="user-avatar"
      (error)="broken.set(true)"
    />
  `,
  styles: `
    :host {
      display: inline-grid;
      place-items: center;
      overflow: hidden;
      border-radius: 999px;
      background: #e4e6eb;
      flex-shrink: 0;
    }
    :host(.is-sm) { width: 2rem; height: 2rem; }
    :host(.is-md) { width: 2.5rem; height: 2.5rem; }
    :host(.is-lg) { width: 4.6rem; height: 4.6rem; }
    :host(.is-xl) { width: 5.6rem; height: 5.6rem; }
    :host(.is-hero) { width: 9.25rem; height: 9.25rem; }
    .user-avatar {
      display: block;
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
  `,
  host: {
    '[class.is-sm]': 'size() === "sm"',
    '[class.is-md]': 'size() === "md"',
    '[class.is-lg]': 'size() === "lg"',
    '[class.is-xl]': 'size() === "xl"',
    '[class.is-hero]': 'size() === "hero"',
  },
})
export class UserAvatarComponent {
  private readonly images = inject(ImageService);

  readonly src = input<string | null | undefined>(null);
  readonly alt = input('');
  readonly size = input<'sm' | 'md' | 'lg' | 'xl' | 'hero'>('md');
  readonly broken = signal(false);
  readonly displaySrc = signal(this.images.emptyAvatar);

  constructor() {
    effect(() => {
      this.broken.set(false);
      this.displaySrc.set(this.images.display(this.src()));
    });

    effect(() => {
      if (this.broken()) this.displaySrc.set(this.images.emptyAvatar);
    });
  }
}
