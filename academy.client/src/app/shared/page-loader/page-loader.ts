import { Component, input } from '@angular/core';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-page-loader',
  standalone: true,
  imports: [TranslatePipe],
  template: `
    <div class="pl" [class.is-compact]="compact()" role="status" aria-live="polite">
      <div class="pl-orb" aria-hidden="true"></div>
      <div class="pl-ring" aria-hidden="true">
        <span></span>
      </div>
      <p>{{ titleKey() | t }}</p>
    </div>
  `,
  styles: `
    .pl {
      display: grid;
      justify-items: center;
      align-content: center;
      gap: 1.1rem;
      min-height: calc(100dvh - 8rem);
      padding: 2.5rem 1rem;
      animation: pl-in 0.35s ease both;
    }

    .pl.is-compact {
      min-height: 12rem;
      padding: 2rem 1rem;
    }

    .pl-orb {
      position: absolute;
      width: 8rem;
      height: 8rem;
      border-radius: 999px;
      background: radial-gradient(circle, rgba(0, 165, 185, 0.18), transparent 70%);
      animation: pl-pulse 1.6s ease-in-out infinite;
    }

    .pl {
      position: relative;
    }

    .pl-ring {
      position: relative;
      z-index: 1;
      width: 3.4rem;
      height: 3.4rem;
    }

    .pl-ring span,
    .pl-ring::before {
      position: absolute;
      inset: 0;
      border-radius: 999px;
      border: 3px solid transparent;
    }

    .pl-ring::before {
      border-color: #d5f1f6;
    }

    .pl-ring span {
      border-top-color: #00a5b9;
      border-right-color: #002d5b;
      animation: pl-spin 0.75s linear infinite;
    }

    .pl p {
      position: relative;
      z-index: 1;
      margin: 0;
      color: #002d5b;
      font-size: 0.98rem;
      font-weight: 800;
      letter-spacing: -0.02em;
    }

    @keyframes pl-spin {
      to {
        transform: rotate(360deg);
      }
    }

    @keyframes pl-pulse {
      0%,
      100% {
        transform: scale(0.86);
        opacity: 0.55;
      }
      50% {
        transform: scale(1.12);
        opacity: 1;
      }
    }

    @keyframes pl-in {
      from {
        opacity: 0;
        transform: translateY(8px);
      }
      to {
        opacity: 1;
        transform: none;
      }
    }
  `,
})
export class PageLoaderComponent {
  readonly titleKey = input.required<string>();
  readonly compact = input(false);
}
