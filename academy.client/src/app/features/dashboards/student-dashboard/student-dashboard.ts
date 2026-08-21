import { Component } from '@angular/core';
import { RoleDashboardComponent } from '../role-dashboard/role-dashboard';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [RoleDashboardComponent],
  template: `
    <app-role-dashboard
      kickerKey="auth.roleStudent"
      titleKey="dashboard.studentTitle"
      subtitleKey="dashboard.studentSub"
    />
  `,
})
export class StudentDashboardComponent {}
