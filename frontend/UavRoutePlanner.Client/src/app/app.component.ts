import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { LogoComponent } from './shared/ui/logo.component';
import { UserMenuComponent } from './shared/ui/user-menu/user-menu.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LogoComponent, UserMenuComponent],
  template: `
    @if (auth.isLoggedIn()) {
      <header class="topbar">
        <a class="brand" routerLink="/missions"><app-logo [size]="36" /><span>UAV Route Planner</span></a>
        <nav>
          <a routerLink="/missions" routerLinkActive="active">Missions</a>
          <a routerLink="/best-time" routerLinkActive="active">Best time</a>
          <a routerLink="/profiles" routerLinkActive="active">UAV profiles</a>
        </nav>
        <app-user-menu />
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
