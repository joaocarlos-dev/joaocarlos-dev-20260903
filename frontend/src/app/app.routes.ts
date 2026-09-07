import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth-guards';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Entrar | Gestão',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login-page').then((module) => module.LoginPage),
  },
  {
    path: '',
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    loadComponent: () =>
      import('./layout/portal-shell/portal-shell').then((module) => module.PortalShell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Visão geral | Gestão',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page').then((module) => module.DashboardPage),
      },
      {
        path: 'usuarios',
        title: 'Usuários | Gestão',
        loadComponent: () =>
          import('./features/users/users-page').then((module) => module.UsersPage),
      },
      {
        path: 'unidades',
        title: 'Unidades | Gestão',
        data: {
          description: 'Organize as unidades e consulte seus colaboradores vinculados.',
          eyebrow: 'Estrutura',
          title: 'Unidades',
        },
        loadComponent: () =>
          import('./features/shared/feature-preview-page').then(
            (module) => module.FeaturePreviewPage,
          ),
      },
      {
        path: 'colaboradores',
        title: 'Colaboradores | Gestão',
        data: {
          description: 'Gerencie vínculos entre pessoas, usuários e unidades.',
          eyebrow: 'Equipe',
          title: 'Colaboradores',
        },
        loadComponent: () =>
          import('./features/shared/feature-preview-page').then(
            (module) => module.FeaturePreviewPage,
          ),
      },
    ],
  },
  {
    path: '**',
    title: 'Página não encontrada | Gestão',
    loadComponent: () =>
      import('./features/not-found/not-found-page').then((module) => module.NotFoundPage),
  },
];
