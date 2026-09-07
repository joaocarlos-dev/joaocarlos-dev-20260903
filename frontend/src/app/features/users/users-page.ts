import { DatePipe, DOCUMENT } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  afterRenderEffect,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';
import { AuthSession } from '../../core/auth/auth-session';
import { EntityStatus, ProblemDetails, UpdateUserRequest, User } from './user.models';
import { UsersApi } from './users-api';

type EditorMode = 'create' | 'edit' | null;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PageHeader, ReactiveFormsModule, ScreenState],
  selector: 'app-users-page',
  templateUrl: './users-page.html',
  styleUrl: './users-page.scss',
})
export class UsersPage {
  private readonly api = inject(UsersApi);
  private readonly session = inject(AuthSession);
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = viewChild<ElementRef<HTMLElement>>('dialog');
  private loadRequestId = 0;
  private returnFocus: HTMLElement | null = null;

  protected readonly users = signal<readonly User[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly editorMode = signal<EditorMode>(null);
  protected readonly selectedUser = signal<User | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly apiFieldErrors = signal<Readonly<Record<string, string>>>({});
  protected readonly feedback = signal<string | null>(null);
  protected readonly canMutate = computed(() => this.session.identity()?.isAdministrator === true);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<'all' | 'active' | 'inactive'>('all');
  protected readonly filteredUsers = computed(() => {
    const term = this.search().trim().toLocaleLowerCase('pt-BR');
    return this.users().filter(
      (user) =>
        !term ||
        user.code.toLocaleLowerCase('pt-BR').includes(term) ||
        user.login.toLocaleLowerCase('pt-BR').includes(term),
    );
  });

  protected readonly createForm = new FormGroup({
    code: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(50)],
    }),
    login: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(100)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.minLength(8)],
    }),
    status: new FormControl<EntityStatus>(1, { nonNullable: true }),
  });

  protected readonly editForm = new FormGroup({
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.pattern(/.*\S.*/), Validators.minLength(8)],
    }),
    status: new FormControl<EntityStatus>(1, { nonNullable: true }),
  });

  constructor() {
    afterRenderEffect(() => {
      const dialog = this.dialog()?.nativeElement;
      if (this.editorMode() && dialog) {
        dialog.querySelector<HTMLElement>('input:not([type="radio"])')?.focus();
      }
    });
    this.destroyRef.onDestroy(() => this.setBackgroundInert(false));
    this.loadUsers();
  }

  @HostListener('document:keydown', ['$event'])
  protected handleDialogKeydown(event: KeyboardEvent): void {
    const dialog = this.dialog()?.nativeElement;
    if (!this.editorMode() || !dialog) {
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      this.closeEditor();
      return;
    }

    if (event.key !== 'Tab') {
      return;
    }

    const controls = Array.from(
      dialog.querySelectorAll<HTMLElement>(
        'button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), a[href]',
      ),
    );
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

  protected loadUsers(): void {
    const requestId = ++this.loadRequestId;
    this.loading.set(true);
    this.loadError.set(null);
    this.api
      .list(this.statusValue())
      .pipe(
        finalize(() => {
          if (requestId === this.loadRequestId) {
            this.loading.set(false);
          }
        }),
      )
      .subscribe({
        next: (users) => {
          if (requestId === this.loadRequestId) {
            this.users.set(users);
          }
        },
        error: (error: unknown) => {
          if (requestId === this.loadRequestId) {
            this.loadError.set(this.listErrorMessage(error));
          }
        },
      });
  }

  protected applySearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected changeStatusFilter(event: Event): void {
    this.statusFilter.set(
      (event.target as HTMLSelectElement).value as 'all' | 'active' | 'inactive',
    );
    this.loadUsers();
  }

  protected openCreate(): void {
    if (!this.canMutate()) {
      return;
    }
    this.returnFocus = this.document.activeElement as HTMLElement | null;
    this.createForm.reset({ code: '', login: '', password: '', status: 1 });
    this.formError.set(null);
    this.apiFieldErrors.set({});
    this.setBackgroundInert(true);
    this.editorMode.set('create');
  }

  protected openEdit(user: User): void {
    if (!this.canMutate()) {
      return;
    }
    this.returnFocus = this.document.activeElement as HTMLElement | null;
    this.selectedUser.set(user);
    this.editForm.reset({ password: '', status: user.status });
    this.formError.set(null);
    this.apiFieldErrors.set({});
    this.setBackgroundInert(true);
    this.editorMode.set('edit');
  }

  protected closeEditor(): void {
    if (this.saving()) {
      return;
    }

    this.editorMode.set(null);
    this.selectedUser.set(null);
    this.formError.set(null);
    this.apiFieldErrors.set({});
    this.setBackgroundInert(false);
    this.document.querySelector<HTMLElement>('.page-content')?.removeAttribute('inert');
    this.returnFocus?.focus();
    this.returnFocus = null;
  }

  protected submitCreate(): void {
    if (!this.canMutate()) {
      return;
    }
    this.formError.set(null);
    this.apiFieldErrors.set({});
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      this.focusFirstInvalid(['code', 'login', 'password']);
      return;
    }

    const value = this.createForm.getRawValue();
    this.saving.set(true);
    this.api
      .create({
        code: value.code.trim(),
        login: value.login.trim(),
        password: value.password,
        status: value.status,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.feedback.set('Usuário criado com sucesso.');
          this.saving.set(false);
          this.closeEditor();
          this.loadUsers();
        },
        error: (error: unknown) => this.handleFormError(error, 'criar'),
      });
  }

  protected submitEdit(): void {
    if (!this.canMutate()) {
      return;
    }
    this.formError.set(null);
    this.apiFieldErrors.set({});
    const user = this.selectedUser();
    if (!user) {
      return;
    }

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.focusFirstInvalid(['password']);
      return;
    }

    const value = this.editForm.getRawValue();
    const request: UpdateUserRequest = {
      ...(value.password ? { password: value.password } : {}),
      ...(value.status !== user.status ? { status: value.status } : {}),
    };
    if (request.password === undefined && request.status === undefined) {
      this.formError.set('Altere a senha ou o status antes de salvar.');
      return;
    }

    this.saving.set(true);
    this.api
      .update(user.id, request)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.feedback.set('Usuário atualizado com sucesso.');
          this.saving.set(false);
          this.closeEditor();
          this.loadUsers();
        },
        error: (error: unknown) => this.handleFormError(error, 'atualizar'),
      });
  }

  private statusValue(): EntityStatus | undefined {
    if (this.statusFilter() === 'active') {
      return 1;
    }

    if (this.statusFilter() === 'inactive') {
      return 0;
    }

    return undefined;
  }

  private errorMessage(error: unknown, action: 'criar' | 'atualizar'): string {
    const problem = this.problemDetails(error);
    let message: string;
    if (error instanceof HttpErrorResponse && error.status === 409) {
      message = 'Já existe um usuário com este código ou login.';
    } else if (error instanceof HttpErrorResponse && error.status === 404) {
      message = 'Este usuário não existe mais. Atualize a lista e tente novamente.';
    } else if (error instanceof HttpErrorResponse && error.status === 400) {
      message = 'Revise os campos informados e tente novamente.';
    } else {
      message = problem?.detail || `Não foi possível ${action} o usuário agora. Tente novamente.`;
    }

    return problem?.traceId ? `${message} Código de suporte: ${problem.traceId}.` : message;
  }

  private listErrorMessage(error: unknown): string {
    const problem = this.problemDetails(error);
    const message = 'Não foi possível carregar os usuários. Tente novamente.';
    return problem?.traceId ? `${message} Código de suporte: ${problem.traceId}.` : message;
  }

  private handleFormError(error: unknown, action: 'criar' | 'atualizar'): void {
    const problem = this.problemDetails(error);
    const fieldErrors: Record<string, string> = {};
    for (const [field, messages] of Object.entries(problem?.errors ?? {})) {
      fieldErrors[field.toLocaleLowerCase('en-US')] = messages.join(' ');
    }
    this.apiFieldErrors.set(fieldErrors);
    this.formError.set(this.errorMessage(error, action));
    const firstField = ['code', 'login', 'password', 'status'].find((field) => fieldErrors[field]);
    if (firstField) {
      const prefix = this.editorMode() === 'create' ? 'create' : 'edit';
      this.document.getElementById(`${prefix}-${firstField}`)?.focus();
    }
  }

  private problemDetails(error: unknown): ProblemDetails | null {
    if (!(error instanceof HttpErrorResponse) || !error.error || typeof error.error !== 'object') {
      return null;
    }

    return error.error as ProblemDetails;
  }

  private focusFirstInvalid(fields: readonly string[]): void {
    const prefix = this.editorMode() === 'create' ? 'create' : 'edit';
    const field = fields.find((name) => {
      const control =
        this.editorMode() === 'create' ? this.createForm.get(name) : this.editForm.get(name);
      return control?.invalid;
    });
    if (field) {
      this.document.getElementById(`${prefix}-${field}`)?.focus();
    }
  }

  private setBackgroundInert(inert: boolean): void {
    this.document.querySelector<HTMLElement>('.header')?.toggleAttribute('inert', inert);
  }
}
