import { Component } from '@angular/core';
import { RoleDashboardComponent } from '../role-dashboard/role-dashboard';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [RoleDashboardComponent],
  template: `
    <app-role-dashboard
      kickerKey="auth.roleTeacher"
      titleKey="dashboard.teacherTitle"
      subtitleKey="dashboard.teacherSub"
    />
  `,
})
export class TeacherDashboardComponent {}
