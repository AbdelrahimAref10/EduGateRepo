import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LandingFooterComponent } from '../landing/landing-footer';
import { LandingNavComponent } from '../landing/landing-nav';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, LandingNavComponent, LandingFooterComponent],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.css',
})
export class PublicLayoutComponent {}
