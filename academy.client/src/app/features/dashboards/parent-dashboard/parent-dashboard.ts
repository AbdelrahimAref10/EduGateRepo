import { Component } from '@angular/core';
import { RoleDashboardComponent } from '../role-dashboard/role-dashboard';

@Component({
  selector: 'app-parent-dashboard',
  standalone: true,
  imports: [RoleDashboardComponent],
  template: `
    <app-role-dashboard
      kickerKey="auth.roleParent"
      titleKey="dashboard.parentTitle"
      subtitleKey="dashboard.parentSub"
    />
  `,
})
export class ParentDashboardComponent {}
