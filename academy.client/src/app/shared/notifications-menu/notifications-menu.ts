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

    const target = this.resolveTarget(item);
    if (target) {
      void this.router.navigate(target.commands, {
        queryParams: target.queryParams,
      });
    }
  }

  @HostListener('document:click')
  close(): void {
    this.open.set(false);
  }

  private resolveTarget(
    item: AppNotification,
  ): { commands: (string | number)[]; queryParams?: Record<string, string | number> } | null {
    const role = this.auth.primaryRole();
    if (!role) return null;

    if (role === 'SuperAdmin') {
      if (item.type === 'LessonGroupEnded') {
        return { commands: ['/super-admin/groups'] };
      }
      if (item.type === 'SessionStarted' && item.entityId) {
        return { commands: ['/super-admin/classroom', item.entityId] };
      }
      if (item.entityType === 'Lesson') {
        return { commands: ['/super-admin/lessons'] };
      }
      return { commands: ['/super-admin'] };
    }

    if (item.type === 'ExamPublished' && item.entityId && role === 'Student') {
      return {
        commands: ['/student/classroom', item.entityId],
        queryParams: { exam: 1 },
      };
    }

    if (item.type === 'ClassroomMaterialAdded' && item.entityId && role === 'Student') {
      return {
        commands: ['/student/classroom', item.entityId],
        queryParams: { materials: 1 },
      };
    }

    if (item.type === 'StudentExamSubmitted' && item.entityId && role === 'Teacher') {
      return {
        commands: ['/teacher/classroom', item.entityId],
        queryParams: item.userTargetId ? { reviewUser: item.userTargetId } : undefined,
      };
    }

    if (item.entityType === 'Lesson' && item.entityId) {
      if (role === 'Teacher') {
        if (item.type === 'LessonBookingRequested') {
          return { commands: ['/teacher/bookings'] };
        }
        return { commands: ['/teacher/lessons', item.entityId] };
      }
      if (role === 'Student') {
        return { commands: ['/student/lessons', item.entityId] };
      }
    }

    if (item.entityType === 'Booking') {
      if (role === 'Teacher') return { commands: ['/teacher/bookings'] };
      if (role === 'Student') return { commands: ['/student/lessons'] };
    }

    if ((item.type === 'StudentAddedToLesson' || item.type === 'StudentAddedToGroup') && item.entityId) {
      if (role === 'Student') return { commands: ['/student/lessons', item.entityId] };
      if (role === 'Teacher') return { commands: ['/teacher/lessons', item.entityId] };
    }

    if (item.type === 'SessionStarted' && item.entityId) {
      if (role === 'Student') return { commands: ['/student/classroom', item.entityId] };
      if (role === 'Teacher') return { commands: ['/teacher/classroom', item.entityId] };
    }

    if (item.type === 'TeacherReviewReceived'
      || item.type === 'LessonReviewReceived'
      || item.type === 'SessionReviewReceived') {
      if (role === 'Teacher') return { commands: ['/teacher/reviews'] };
    }

    return null;
  }
}
