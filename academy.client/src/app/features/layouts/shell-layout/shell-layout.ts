import { Component, OnInit, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { NotificationService } from '../../../core/notifications/notification.service';
import { LanguageSwitcherComponent } from '../../../shared/language-switcher/language-switcher';
import { NotificationsMenuComponent } from '../../../shared/notifications-menu/notifications-menu';
import { UserMenuComponent } from '../../../shared/user-menu/user-menu';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog';

type Accent = 'admin' | 'teacher' | 'student' | 'parent';

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    TranslatePipe,
    LanguageSwitcherComponent,
    NotificationsMenuComponent,
    UserMenuComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './shell-layout.html',
  styleUrl: './shell-layout.css',
})
export class ShellLayoutComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly notifications = inject(NotificationService);

  ngOnInit(): void {
    this.notifications.startRealtime();
  }

  private readonly routeData = toSignal(
    this.route.data.pipe(
      map((data) => ({
        roleKey: (data['roleKey'] as string) ?? 'shell.workspace',
        accent: (data['accent'] as Accent) ?? 'teacher',
        homeLink: (data['homeLink'] as string) ?? '/',
      })),
    ),
    {
      initialValue: {
        roleKey: 'shell.workspace',
        accent: 'teacher' as Accent,
        homeLink: '/',
      },
    },
  );

  readonly roleKey = computed(() => this.routeData().roleKey);
  readonly accent = computed(() => this.routeData().accent);
  readonly homeLink = computed(() => this.routeData().homeLink);
  readonly fullName = this.auth.fullName;
}
