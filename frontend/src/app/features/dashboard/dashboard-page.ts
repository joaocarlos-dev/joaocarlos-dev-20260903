import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { EmployeesApi } from '../employees/employees-api';
import { UnitsApi } from '../units/units-api';
import { UsersApi } from '../users/users-api';
import { ProblemDetails } from '../../core/api/api.models';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PageHeader, RouterLink, ScreenState],
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  private readonly employeesApi = inject(EmployeesApi);
  private readonly unitsApi = inject(UnitsApi);
  private readonly usersApi = inject(UsersApi);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly totals = signal({ employees: 0, units: 0, users: 0 });
  private loadRequestId = 0;
  protected readonly cards = [
    { label: 'Usuários', path: '/usuarios' },
    { label: 'Unidades', path: '/unidades' },
    { label: 'Colaboradores', path: '/colaboradores' },
  ] as const;

  protected readonly cardsWithTotals = computed(() => [
    { ...this.cards[0], total: this.totals().users },
    { ...this.cards[1], total: this.totals().units },
    { ...this.cards[2], total: this.totals().employees },
  ]);

  constructor() {
    this.loadTotals();
  }

  protected loadTotals(): void {
    const requestId = ++this.loadRequestId;
    this.loading.set(true);
    this.error.set(null);
    forkJoin({ employees: this.employeesApi.list(), units: this.unitsApi.list(), users: this.usersApi.list() })
      .pipe(finalize(() => { if (requestId === this.loadRequestId) this.loading.set(false); }))
      .subscribe({
        next: ({ employees, units, users }) => {
          if (requestId === this.loadRequestId) this.totals.set({ employees: employees.length, units: units.length, users: users.length });
        },
        error: (error: unknown) => {
          if (requestId !== this.loadRequestId) return;
          const problem = error instanceof HttpErrorResponse && error.error && typeof error.error === 'object' ? error.error as ProblemDetails : null;
          this.error.set(`${problem?.detail ?? 'Não foi possível carregar os totais agora.'}${problem?.traceId ? ` Código de suporte: ${problem.traceId}.` : ''}`);
        },
      });
  }
}
