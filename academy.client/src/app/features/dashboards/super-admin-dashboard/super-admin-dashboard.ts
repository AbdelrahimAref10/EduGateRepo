import { Component } from '@angular/core';
import { RoleDashboardComponent } from '../role-dashboard/role-dashboard';

@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [RoleDashboardComponent],
  template: `
    <app-role-dashboard
      kickerKey="auth.roleAdmin"
      titleKey="dashboard.adminTitle"
      subtitleKey="dashboard.adminSub"
    />
  `,
})
export class SuperAdminDashboardComponent {}
