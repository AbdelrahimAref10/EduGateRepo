import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { loginQueryParams } from '../../../core/auth/return-url';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { LanguageSwitcherComponent } from '../../../shared/language-switcher/language-switcher';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, LanguageSwitcherComponent],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly returnUrl = computed(() => this.route.snapshot.queryParamMap.get('returnUrl'));
  readonly registerQueryParams = computed(() => loginQueryParams(this.returnUrl()));

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  submit(): void {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.auth.login(this.form.getRawValue(), this.returnUrl()).subscribe({
      next: () => this.loading.set(false),
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail || err?.error?.title || 'Unable to sign in.');
      },
    });
  }
}
