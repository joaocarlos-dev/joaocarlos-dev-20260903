import { DatePipe, DOCUMENT } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { afterRenderEffect, ChangeDetectionStrategy, Component, computed, DestroyRef, ElementRef, HostListener, inject, signal, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { ProblemDetails } from '../../core/api/api.models';
import { User } from '../users/user.models';
import { UsersApi } from '../users/users-api';
import { Unit } from '../units/unit.models';
import { UnitsApi } from '../units/units-api';
import { CreateEmployeeRequest, Employee, UpdateEmployeeRequest } from './employee.models';
import { EmployeesApi } from './employees-api';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';
import { AuthSession } from '../../core/auth/auth-session';

type EditorMode = 'create' | 'edit' | null;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PageHeader, ReactiveFormsModule, RouterLink, ScreenState],
  selector: 'app-employees-page',
  templateUrl: './employees-page.html',
  styleUrl: './employees-page.scss',
})
export class EmployeesPage {
  private readonly api = inject(EmployeesApi);
  private readonly session = inject(AuthSession);
  private readonly usersApi = inject(UsersApi);
  private readonly unitsApi = inject(UnitsApi);
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = viewChild<ElementRef<HTMLElement>>('dialog');
  private readonly pageContent = viewChild<ElementRef<HTMLElement>>('pageContent');
  private returnFocus: HTMLElement | null = null;
  private loadRequestId = 0;

  protected readonly employees = signal<readonly Employee[]>([]);
  protected readonly users = signal<readonly User[]>([]);
  protected readonly units = signal<readonly Unit[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly editorMode = signal<EditorMode>(null);
  protected readonly selectedEmployee = signal<Employee | null>(null);
  protected readonly saving = signal(false);
  protected readonly deleting = signal<string | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly feedback = signal<string | null>(null);
  protected readonly canMutate = computed(() => this.session.identity()?.isAdministrator === true);
  protected readonly search = signal('');
  protected readonly filteredEmployees = computed(() => {
    const term = this.search().trim().toLocaleLowerCase('pt-BR');
    return this.employees().filter((employee) =>
      [employee.code, employee.name, this.userName(employee.userId), this.unitName(employee.unitId)].some(
        (value) => value.toLocaleLowerCase('pt-BR').includes(term),
      ),
    );
  });
  protected readonly hasSearch = computed(() => !!this.search().trim());

  protected readonly createForm = new FormGroup({
    code: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(50)] }),
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(150)] }),
    userId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    unitId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });
  protected readonly editForm = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(150)] }),
    unitId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  constructor() {
    afterRenderEffect(() => {
      if (this.editorMode()) this.dialog()?.nativeElement.querySelector<HTMLElement>('input, select')?.focus();
    });
    this.destroyRef.onDestroy(() => this.setBackgroundInert(false));
    this.loadData();
  }

  @HostListener('document:keydown', ['$event'])
  protected handleDialogKeydown(event: KeyboardEvent): void {
    const dialog = this.dialog()?.nativeElement;
    if (!this.editorMode() || !dialog) return;
    if (event.key === 'Escape') {
      event.preventDefault();
      this.closeEditor();
      return;
    }
    if (event.key !== 'Tab') return;
    const controls = Array.from(dialog.querySelectorAll<HTMLElement>('button:not([disabled]), input:not([disabled]), select:not([disabled])'));
    const first = controls[0];
    const last = controls[controls.length - 1];
    if (event.shiftKey && this.document.activeElement === first) {
      event.preventDefault();
      last?.focus();
    } else if (!event.shiftKey && this.document.activeElement === last) {
      event.preventDefault();
      first?.focus();
    }
  }

  protected loadData(): void {
    const requestId = ++this.loadRequestId;
    this.loading.set(true);
    this.loadError.set(null);
    forkJoin({ employees: this.api.list(), users: this.usersApi.list(), units: this.unitsApi.list() })
      .pipe(finalize(() => { if (requestId === this.loadRequestId) this.loading.set(false); }))
      .subscribe({
        next: ({ employees, users, units }) => {
          if (requestId !== this.loadRequestId) return;
          this.employees.set(employees);
          this.users.set(users);
          this.units.set(units);
        },
        error: (error: unknown) => { if (requestId === this.loadRequestId) this.loadError.set(this.errorMessage(error, 'carregar os colaboradores')); },
      });
  }

  protected applySearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected openCreate(): void {
    if (!this.canMutate()) return;
    this.returnFocus = this.document.activeElement as HTMLElement | null;
    this.createForm.reset({ code: '', name: '', userId: '', unitId: '' });
    this.formError.set(null);
    this.setBackgroundInert(true);
    this.editorMode.set('create');
  }

  protected openEdit(employee: Employee): void {
    if (!this.canMutate()) return;
    this.returnFocus = this.document.activeElement as HTMLElement | null;
    this.selectedEmployee.set(employee);
    this.editForm.reset({ name: employee.name, unitId: employee.unitId });
    this.formError.set(null);
    this.setBackgroundInert(true);
    this.editorMode.set('edit');
  }

  protected closeEditor(): void {
    if (this.saving()) return;
    this.editorMode.set(null);
    this.setBackgroundInert(false);
    this.returnFocus?.focus();
    this.returnFocus = null;
  }

  protected submitCreate(): void {
    if (!this.canMutate()) return;
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    const value = this.createForm.getRawValue();
    const request: CreateEmployeeRequest = { code: value.code.trim(), name: value.name.trim(), userId: value.userId, unitId: value.unitId };
    this.saving.set(true);
    this.api.create(request).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => { this.feedback.set('Colaborador criado com sucesso.'); this.saving.set(false); this.closeEditor(); this.loadData(); },
      error: (error: unknown) => this.formError.set(this.errorMessage(error, 'criar o colaborador')),
    });
  }

  protected submitEdit(): void {
    if (!this.canMutate()) return;
    const employee = this.selectedEmployee();
    if (!employee || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const value = this.editForm.getRawValue();
    const request: UpdateEmployeeRequest = {
      ...(value.name.trim() !== employee.name ? { name: value.name.trim() } : {}),
      ...(value.unitId !== employee.unitId ? { unitId: value.unitId } : {}),
    };
    if (!request.name && !request.unitId) {
      this.formError.set('Altere o nome ou a unidade antes de salvar.');
      return;
    }
    this.saving.set(true);
    this.api.update(employee.id, request).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => { this.feedback.set('Colaborador atualizado com sucesso.'); this.saving.set(false); this.closeEditor(); this.loadData(); },
      error: (error: unknown) => this.formError.set(this.errorMessage(error, 'atualizar o colaborador')),
    });
  }

  protected remove(employee: Employee): void {
    if (!this.canMutate()) return;
    if (!this.document.defaultView?.confirm(`Remover o colaborador ${employee.name}?`)) return;
    this.deleting.set(employee.id);
    this.api.delete(employee.id).pipe(finalize(() => this.deleting.set(null))).subscribe({
      next: () => { this.feedback.set('Colaborador removido com sucesso.'); this.loadData(); },
      error: (error: unknown) => this.feedback.set(this.errorMessage(error, 'remover o colaborador')),
    });
  }

  protected userName(id: string): string { return this.users().find((user) => user.id === id)?.login ?? 'Usuário não encontrado'; }
  protected unitName(id: string): string { return this.units().find((unit) => unit.id === id)?.name ?? 'Unidade não encontrada'; }
  protected availableUsers(): readonly User[] {
    const used = new Set(this.employees().map((employee) => employee.userId));
    return this.users().filter((user) => user.status === 1 && !used.has(user.id));
  }
  protected activeUnits(): readonly Unit[] { return this.units().filter((unit) => unit.status === 1); }

  private errorMessage(error: unknown, action: string): string {
    const problem = error instanceof HttpErrorResponse && error.error && typeof error.error === 'object' ? error.error as ProblemDetails : null;
    if (error instanceof HttpErrorResponse && error.status === 409) return 'A operação entrou em conflito com os dados atuais. Verifique o código, o usuário e a unidade.';
    if (error instanceof HttpErrorResponse && error.status === 404) return 'O colaborador, usuário ou unidade não foi encontrado.';
    if (error instanceof HttpErrorResponse && error.status === 400) return 'Revise os campos informados e tente novamente.';
    return problem?.detail ?? `Não foi possível ${action}. Tente novamente.`;
  }

  private setBackgroundInert(inert: boolean): void {
    this.document.querySelector<HTMLElement>('.header')?.toggleAttribute('inert', inert);
    this.pageContent()?.nativeElement.toggleAttribute('inert', inert);
  }
}
