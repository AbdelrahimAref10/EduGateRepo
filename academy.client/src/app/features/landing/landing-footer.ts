import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-landing-footer',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './landing-footer.html',
  styleUrl: './landing-footer.css',
})
export class LandingFooterComponent {
  readonly year = new Date().getFullYear();
}
