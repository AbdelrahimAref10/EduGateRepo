import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-rating-stars',
  standalone: true,
  template: `
    <span class="stars" [attr.aria-label]="label || null">
      @for (star of [1, 2, 3, 4, 5]; track star) {
        <span [class.on]="star <= filled">★</span>
      }
    </span>
  `,
  styles: `
    .stars {
      display: inline-flex;
      gap: 0.08rem;
      letter-spacing: 0.02em;
      line-height: 1;
    }

    .stars span {
      color: #c4ccd4;
      font-size: 0.95rem;
    }

    .stars span.on {
      color: #d97706;
    }
  `,
})
export class RatingStarsComponent {
  @Input() filled = 0;
  @Input() label = '';
}
