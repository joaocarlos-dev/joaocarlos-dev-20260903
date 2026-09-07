import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { distinctUntilChanged, finalize, forkJoin, map } from 'rxjs';
import { ProblemDetails } from '../../core/api/api.models';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';
import { User } from '../users/user.models';
import { UsersApi } from '../users/users-api';
import { Unit } from '../units/unit.models';
import { UnitsApi } from '../units/units-api';
import { Employee } from './employee.models';
import { EmployeesApi } from './employees-api';

interface DetailError { readonly title: string; readonly description: string; readonly notFound: boolean; }

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PageHeader, RouterLink, ScreenState],
  selector: 'app-employee-details-page',
  template: `
    @if (loading()) { <app-screen-state kind="loading" title="Carregando colaborador..." description="Consultando os dados do vínculo." /> }
    @else if (error()) { <app-screen-state kind="error" [title]="error()!.title" [description]="error()!.description" [actionLabel]="error()!.notFound ? 'Voltar para colaboradores' : 'Tentar novamente'" (action)="handleErrorAction()" /> }
    @else if (employee()) {
      <app-page-header eyebrow="Equipe" [title]="employee()!.name" [description]="'Detalhes do colaborador ' + employee()!.code + '.'"><a class="button" routerLink="/colaboradores">Voltar</a></app-page-header>
      <section class="card"><dl><dt>Código</dt><dd>{{ employee()!.code }}</dd><dt>Usuário</dt><dd>{{ userName() }}</dd><dt>Unidade</dt><dd>{{ unitName() }}</dd><dt>Criado em</dt><dd>{{ employee()!.createdAt | date: 'dd/MM/yyyy HH:mm' }}</dd></dl></section>
    }
  `,
  styles: [`.card { padding: 1.5rem; border: 1px solid var(--line); border-radius: .75rem; background: var(--surface); } dl { display: grid; grid-template-columns: minmax(8rem, 12rem) 1fr; gap: 1rem; margin: 0; } dt { color: var(--ink-muted); } dd { margin: 0; }`],
})
export class EmployeeDetailsPage {
  private readonly api = inject(EmployeesApi);
  private readonly usersApi = inject(UsersApi);
  private readonly unitsApi = inject(UnitsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private id = '';
  private loadRequestId = 0;
  protected readonly employee = signal<Employee | null>(null);
  protected readonly users = signal<readonly User[]>([]);
  protected readonly units = signal<readonly Unit[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<DetailError | null>(null);

  constructor() {
    this.route.paramMap.pipe(map((params) => params.get('id') ?? ''), distinctUntilChanged(), takeUntilDestroyed()).subscribe((id) => {
      this.id = id;
      this.load();
    });
  }

  protected load(): void {
    const requestId = ++this.loadRequestId;
    this.loading.set(true);
    this.error.set(null);
    forkJoin({ employee: this.api.get(this.id), users: this.usersApi.list(), units: this.unitsApi.list() }).pipe(finalize(() => { if (requestId === this.loadRequestId) this.loading.set(false); })).subscribe({
      next: ({ employee, users, units }) => { if (requestId !== this.loadRequestId) return; this.employee.set(employee); this.users.set(users); this.units.set(units); },
      error: (error: unknown) => {
        if (requestId !== this.loadRequestId) return;
        const problem = error instanceof HttpErrorResponse && error.error && typeof error.error === 'object' ? error.error as ProblemDetails : null;
        const notFound = error instanceof HttpErrorResponse && error.status === 404;
        this.error.set({ notFound, title: notFound ? 'Colaborador não encontrado' : 'Não foi possível carregar o colaborador', description: problem?.detail ?? (notFound ? 'O colaborador não existe mais.' : 'Tente novamente ou volte para a listagem.') });
      },
    });
  }

  protected handleErrorAction(): void {
    if (this.error()?.notFound) void this.router.navigate(['/colaboradores']);
    else this.load();
  }

  protected userName(): string { return this.users().find((user) => user.id === this.employee()?.userId)?.login ?? 'Usuário não encontrado'; }
  protected unitName(): string { return this.units().find((unit) => unit.id === this.employee()?.unitId)?.name ?? 'Unidade não encontrada'; }
}
