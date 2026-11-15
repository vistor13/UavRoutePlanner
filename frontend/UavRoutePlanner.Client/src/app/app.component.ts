import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.isLoggedIn()) {
      <header class="topbar">
        <span class="brand">UAV Route Planner</span>
        <nav>
          <a routerLink="/missions" routerLinkActive="active">Missions</a>
          <a routerLink="/profiles" routerLinkActive="active">UAV profiles</a>
        </nav>
        <button class="link" type="button" (click)="auth.logout()">Sign out</button>
      </header>
    }
    <main>
      <router-outlet />
    </main>
  `
})
export class AppComponent {
  protected readonly auth = inject(AuthService);
}
