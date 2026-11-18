import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'missions' },
      {
        path: 'missions',
        loadComponent: () => import('./features/missions/missions.component').then(m => m.MissionsComponent)
      },
      {
        path: 'missions/new',
        loadComponent: () =>
          import('./features/missions/editor/mission-editor.component').then(m => m.MissionEditorComponent)
      },
      {
        path: 'missions/:id/edit',
        loadComponent: () =>
          import('./features/missions/editor/mission-editor.component').then(m => m.MissionEditorComponent)
      },
      {
        path: 'missions/:id',
        loadComponent: () =>
          import('./features/missions/view/mission-view.component').then(m => m.MissionViewComponent)
      },
      {
        path: 'best-time',
        loadComponent: () => import('./features/best-time/best-time.component').then(m => m.BestTimeComponent)
      },
      {
        path: 'profiles',
        loadComponent: () => import('./features/profiles/profiles.component').then(m => m.ProfilesComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
