import { Component, HostListener, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import {
  AppNotification,
  NotificationService,
} from '../../core/notifications/notification.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-notifications-menu',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './notifications-menu.html',
  styleUrl: './notifications-menu.css',
})
export class NotificationsMenuComponent {
  private readonly notifications = inject(NotificationService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly open = signal(false);
  // Re-wrap so this component's template always tracks live service signals.
  readonly items = computed(() => this.notifications.items());
  readonly unreadCount = computed(() => this.notifications.unreadCount());

  toggle(event: MouseEvent): void {
    event.stopPropagation();
    this.notifications.unlockAudio();
    const next = !this.open();
    this.open.set(next);
    if (next) {
      this.notifications.refresh();
    }
  }

  markAll(event: MouseEvent): void {
    event.stopPropagation();
    this.notifications.unlockAudio();
    this.notifications.markAllRead();
  }

  openItem(item: AppNotification, event: MouseEvent): void {
    event.stopPropagation();
    this.notifications.unlockAudio();
    this.notifications.markRead(item.id);
    this.open.set(false);

    const commands = this.resolveRoute(item);
    if (commands) {
      void this.router.navigate(commands);
    }
  }

  @HostListener('document:click')
  close(): void {
    this.open.set(false);
  }

  private resolveRoute(item: AppNotification): (string | number)[] | null {
    const role = this.auth.primaryRole();
    if (!role) return null;

    if (role === 'SuperAdmin') {
      return ['/super-admin'];
    }

    if (item.entityType === 'Lesson' && item.entityId) {
      if (role === 'Teacher') {
        if (item.type === 'LessonBookingRequested') {
          return ['/teacher/bookings'];
        }
        return ['/teacher/lessons', item.entityId];
      }
      if (role === 'Student') {
        return ['/student/lessons', item.entityId];
      }
    }

    if (item.entityType === 'Booking') {
      if (role === 'Teacher') return ['/teacher/bookings'];
      if (role === 'Student') return ['/student/lessons'];
    }

    if (item.type === 'StudentAddedToLesson' && item.entityId) {
      if (role === 'Student') return ['/student/lessons', item.entityId];
      if (role === 'Teacher') return ['/teacher/lessons', item.entityId];
    }

    return null;
  }
}
