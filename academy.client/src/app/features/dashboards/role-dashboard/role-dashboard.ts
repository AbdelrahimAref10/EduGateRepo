import { Component, Input, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

type Accent = 'admin' | 'teacher' | 'student' | 'parent';

@Component({
  selector: 'app-role-dashboard',
  standalone: true,
  imports: [TranslatePipe, RouterLink],
  templateUrl: './role-dashboard.html',
  styleUrl: './role-dashboard.css',
})
export class RoleDashboardComponent {
  @Input({ required: true }) titleKey!: string;
  @Input({ required: true }) subtitleKey!: string;
  @Input() kickerKey = 'shell.workspace';

  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);

  private readonly accentSignal = toSignal(
    this.route.parent!.data.pipe(map((data) => (data['accent'] as Accent) ?? 'teacher')),
    { initialValue: 'teacher' as Accent },
  );

  readonly accent = computed(() => this.accentSignal());

  readonly quickLinks = computed(() => {
    switch (this.accent()) {
      case 'admin': {
        const links = [
          { labelKey: 'adminLessons.nav', link: '/super-admin/lessons' },
          { labelKey: 'adminGroups.nav', link: '/super-admin/groups' },
          { labelKey: 'education.nav', link: '/super-admin/education' },
          { labelKey: 'countries.nav', link: '/super-admin/countries' },
          { labelKey: 'common.profile', link: '/super-admin/profile' },
        ];
        if (this.auth.canManageUsers()) {
          return [{ labelKey: 'adminUsers.nav', link: '/super-admin/users' }, ...links];
        }
        return links;
      }
      case 'teacher':
        return [
          { labelKey: 'lessons.nav', link: '/teacher/lessons' },
          { labelKey: 'booking.nav', link: '/teacher/bookings' },
          { labelKey: 'common.profile', link: '/teacher/profile' },
        ];
      case 'student':
        return [
          { labelKey: 'studentLessons.nav', link: '/student/lessons' },
          { labelKey: 'booking.studentNav', link: '/student/book' },
          { labelKey: 'common.profile', link: '/student/profile' },
        ];
      default:
        return [{ labelKey: 'common.profile', link: '/parent/profile' }];
    }
  });

  readonly stats = [
    {
      labelKey: 'dashboard.statStudents',
      value: '2,301',
      iconClass: 'bg-gradient-to-br from-primary-500 to-primary-700',
      iconPath: 'M16 19v-1.2A3.8 3.8 0 0 0 12.2 14H7.8A3.8 3.8 0 0 0 4 17.8V19M14.5 8.5a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0ZM20 19v-1a3 3 0 0 0-2.2-2.9M17.5 8.6a2.2 2.2 0 0 1 0 4.2',
    },
    {
      labelKey: 'dashboard.statCourses',
      value: '18',
      iconClass: 'bg-gradient-to-br from-info to-primary-600',
      iconPath: 'M5 5.5A2.5 2.5 0 0 1 7.5 3H19v15H7.5A2.5 2.5 0 0 0 5 20.5V5.5ZM5 18.5h14',
    },
    {
      labelKey: 'dashboard.statSessions',
      value: '12',
      iconClass: 'bg-gradient-to-br from-warning to-orange-500',
      iconPath: 'M8 3.5v3M16 3.5v3M4.5 9.5h15M6 6h12a1.5 1.5 0 0 1 1.5 1.5v11A1.5 1.5 0 0 1 18 20H6a1.5 1.5 0 0 1-1.5-1.5v-11A1.5 1.5 0 0 1 6 6Z',
    },
    {
      labelKey: 'dashboard.statAlerts',
      value: '3',
      iconClass: 'bg-gradient-to-br from-danger to-rose-500',
      iconPath: 'M12 9v4M12 17h.01M10.3 4.2 2.8 17.1A2 2 0 0 0 4.5 20h15a2 2 0 0 0 1.7-2.9L13.7 4.2a2 2 0 0 0-3.4 0Z',
    },
  ];

  readonly bars = [42, 68, 55, 80, 64, 92, 74];

  readonly meters = [
    { labelKey: 'dashboard.statCourses', value: 72 },
    { labelKey: 'dashboard.statStudents', value: 58 },
    { labelKey: 'dashboard.statSessions', value: 91 },
  ];
}
