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
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { EntityStatus, ProblemDetails } from '../../core/api/api.models';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';
import { AuthSession } from '../../core/auth/auth-session';
import { Unit, UpdateUnitRequest } from './unit.models';
import { UnitsApi } from './units-api';

type EditorMode = 'create' | 'edit' | null;

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PageHeader, ReactiveFormsModule, RouterLink, ScreenState],
  selector: 'app-units-page',
  templateUrl: './units-page.html',
  styleUrl: './units-page.scss',
})
export class UnitsPage {
  private readonly api = inject(UnitsApi);
  private readonly session = inject(AuthSession);
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = viewChild<ElementRef<HTMLElement>>('dialog');
  private loadRequestId = 0;
  private returnFocus: HTMLElement | null = null;
  private feedbackTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly units = signal<readonly Unit[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly editorMode = signal<EditorMode>(null);
  protected readonly selectedUnit = signal<Unit | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly apiFieldErrors = signal<Readonly<Record<string, string>>>({});
  protected readonly feedback = signal<string | null>(null);
  protected readonly canMutate = computed(() => this.session.identity()?.isAdministrator === true);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<'all' | 'active' | 'inactive'>('all');
  protected readonly filteredUnits = computed(() => {
    const term = this.search().trim().toLocaleLowerCase('pt-BR');
    const status = this.statusFilter();
    return this.units().filter(
      (unit) =>
        (status === 'all' || (status === 'active' ? unit.status === 1 : unit.status === 0)) &&
        (!term ||
          unit.code.toLocaleLowerCase('pt-BR').includes(term) ||
          unit.name.toLocaleLowerCase('pt-BR').includes(term)),
    );
  });
  protected readonly hasFilters = computed(
    () => !!this.search().trim() || this.statusFilter() !== 'all',
  );

  protected readonly createForm = new FormGroup({
    code: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(50)],
    }),
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(150)],
    }),
  });

  protected readonly editForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/.*\S.*/), Validators.maxLength(150)],
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
    this.destroyRef.onDestroy(() => {
      this.setBackgroundInert(false);
      if (this.feedbackTimer) {
        clearTimeout(this.feedbackTimer);
      }
    });
    this.loadUnits();
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
        'button:not([disabled]), input:not([disabled]), select:not([disabled]), a[href]',
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

  protected loadUnits(): void {
    const requestId = ++this.loadRequestId;
    this.loading.set(true);
    this.loadError.set(null);
    this.api
      .list()
      .pipe(
        finalize(() => {
          if (requestId === this.loadRequestId) {
            this.loading.set(false);
          }
        }),
      )
      .subscribe({
        next: (units) => {
          if (requestId === this.loadRequestId) {
            this.units.set(units);
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
  }

  protected openCreate(): void {
    if (!this.canMutate()) return;
    this.returnFocus = this.document.activeElement as HTMLElement | null;
    this.createForm.reset({ code: '', name: '' });
    this.formError.set(null);
    this.apiFieldErrors.set({});
    this.setBackgroundInert(true);
    this.editorMode.set('create');
  }

  protected openEdit(unit: Unit): void {
    if (!this.canMutate()) return;
    this.returnFocus = this.document.activeElement as HTMLElement | null;
    this.selectedUnit.set(unit);
    this.editForm.reset({ name: unit.name, status: unit.status });
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
    this.selectedUnit.set(null);
    this.formError.set(null);
    this.apiFieldErrors.set({});
    this.setBackgroundInert(false);
    this.returnFocus?.focus();
    this.returnFocus = null;
  }

  protected submitCreate(): void {
    if (!this.canMutate()) return;
    this.formError.set(null);
    this.apiFieldErrors.set({});
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      this.focusFirstInvalid(['code', 'name']);
      return;
    }

    const value = this.createForm.getRawValue();
    this.saving.set(true);
    this.api
      .create({ code: value.code.trim(), name: value.name.trim() })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.showFeedback('Unidade criada com sucesso.');
          this.saving.set(false);
          this.closeEditor();
          this.loadUnits();
        },
        error: (error: unknown) => this.handleFormError(error, 'criar'),
      });
  }

  protected submitEdit(): void {
    if (!this.canMutate()) return;
    this.formError.set(null);
    this.apiFieldErrors.set({});
    const unit = this.selectedUnit();
    if (!unit) {
      return;
    }

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      this.focusFirstInvalid(['name']);
      return;
    }

    const value = this.editForm.getRawValue();
    const request: UpdateUnitRequest = {
      ...(value.name.trim() !== unit.name ? { name: value.name.trim() } : {}),
      ...(value.status !== unit.status ? { status: value.status } : {}),
    };
    if (request.name === undefined && request.status === undefined) {
      this.formError.set('Altere o nome ou o status antes de salvar.');
      return;
    }

    this.saving.set(true);
    this.api
      .update(unit.id, request)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.showFeedback('Unidade atualizada com sucesso.');
          this.saving.set(false);
          this.closeEditor();
          this.loadUnits();
        },
        error: (error: unknown) => this.handleFormError(error, 'atualizar'),
      });
  }

  private errorMessage(error: unknown, action: 'criar' | 'atualizar'): string {
    const problem = this.problemDetails(error);
    let message: string;
    if (error instanceof HttpErrorResponse && error.status === 409) {
      message = 'Já existe uma unidade com este código.';
    } else if (error instanceof HttpErrorResponse && error.status === 404) {
      message = 'Esta unidade não existe mais. Atualize a lista e tente novamente.';
    } else if (error instanceof HttpErrorResponse && error.status === 400) {
      message = 'Revise os campos informados e tente novamente.';
    } else {
      message = problem?.detail || `Não foi possível ${action} a unidade agora. Tente novamente.`;
    }
    return problem?.traceId ? `${message} Código de suporte: ${problem.traceId}.` : message;
  }

  private listErrorMessage(error: unknown): string {
    const problem = this.problemDetails(error);
    const message = 'Não foi possível carregar as unidades. Tente novamente.';
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
    const firstField = ['code', 'name', 'status'].find((field) => fieldErrors[field]);
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

  private showFeedback(message: string): void {
    if (this.feedbackTimer) {
      clearTimeout(this.feedbackTimer);
    }
    this.feedback.set(message);
    this.feedbackTimer = setTimeout(() => {
      this.feedback.set(null);
      this.feedbackTimer = null;
    }, 2000);
  }
}
