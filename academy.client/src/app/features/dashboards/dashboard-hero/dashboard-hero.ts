import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-dashboard-hero',
  standalone: true,
  templateUrl: './dashboard-hero.html',
  styleUrl: './dashboard-hero.css',
})
export class DashboardHeroComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) subtitle!: string;
  @Input() kicker = 'Workspace';
}
