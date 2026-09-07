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
        loadComponent: () =>
          import('./features/units/units-page').then((module) => module.UnitsPage),
      },
      {
        path: 'unidades/:id',
        title: 'Detalhes da unidade | Gestão',
        loadComponent: () =>
          import('./features/units/unit-details-page').then(
            (module) => module.UnitDetailsPage,
          ),
      },
      {
        path: 'colaboradores',
        title: 'Colaboradores | Gestão',
        loadComponent: () =>
          import('./features/employees/employees-page').then((module) => module.EmployeesPage),
      },
      {
        path: 'colaboradores/:id',
        title: 'Detalhes do colaborador | Gestão',
        loadComponent: () =>
          import('./features/employees/employee-details-page').then(
            (module) => module.EmployeeDetailsPage,
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
