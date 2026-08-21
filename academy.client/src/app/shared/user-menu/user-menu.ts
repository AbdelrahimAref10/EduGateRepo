import { Component, HostListener, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [RouterLink, TranslatePipe],
  templateUrl: './user-menu.html',
  styleUrl: './user-menu.css',
  host: {
    '[class.is-open]': 'open()',
  },
})
export class UserMenuComponent {
  private readonly auth = inject(AuthService);

  readonly open = signal(false);
  readonly fullName = this.auth.fullName;
  readonly email = computed(() => this.auth.session()?.email ?? '');
  readonly initials = computed(() => {
    const name = this.fullName().trim();
    if (!name) return 'A';
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  });

  toggle(event: MouseEvent): void {
    event.stopPropagation();
    this.open.update((value) => !value);
  }

  logout(event: MouseEvent): void {
    event.stopPropagation();
    this.open.set(false);
    this.auth.logout();
  }

  @HostListener('document:click')
  close(): void {
    this.open.set(false);
  }
}
