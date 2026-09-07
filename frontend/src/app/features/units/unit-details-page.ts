import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { distinctUntilChanged, finalize, map } from 'rxjs';
import { ProblemDetails } from '../../core/api/api.models';
import { PageHeader } from '../../shared/page-header/page-header';
import { ScreenState } from '../../shared/screen-state/screen-state';
import { Unit } from './unit.models';
import { UnitsApi } from './units-api';

interface DetailError {
  readonly description: string;
  readonly notFound: boolean;
  readonly title: string;
}

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, PageHeader, RouterLink, ScreenState],
  selector: 'app-unit-details-page',
  templateUrl: './unit-details-page.html',
  styleUrl: './unit-details-page.scss',
})
export class UnitDetailsPage {
  private readonly api = inject(UnitsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly unitId = signal('');
  private loadRequestId = 0;

  protected readonly unit = signal<Unit | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal<DetailError | null>(null);
  protected readonly pageTitle = computed(() => this.unit()?.name ?? 'Detalhes da unidade');
  protected readonly pageDescription = computed(() => {
    const unit = this.unit();
    return unit
      ? `Consulte os dados e os colaboradores vinculados à unidade ${unit.code}.`
      : 'Consulte os dados e os colaboradores vinculados a esta unidade.';
  });

  constructor() {
    this.route.paramMap
      .pipe(
        map((params) => params.get('id') ?? ''),
        distinctUntilChanged(),
        takeUntilDestroyed(),
      )
      .subscribe((id) => {
        this.unitId.set(id);
        this.loadUnit();
      });
  }

  protected loadUnit(): void {
    const id = this.unitId();
    const requestId = ++this.loadRequestId;
    this.unit.set(null);
    this.loadError.set(null);

    if (!id) {
      this.loading.set(false);
      this.loadError.set({
        description: 'O endereço informado não identifica uma unidade válida.',
        notFound: true,
        title: 'Unidade não encontrada',
      });
      return;
    }

    this.loading.set(true);
    this.api
      .get(id)
      .pipe(
        finalize(() => {
          if (requestId === this.loadRequestId) {
            this.loading.set(false);
          }
        }),
      )
      .subscribe({
        next: (unit) => {
          if (requestId === this.loadRequestId) {
            this.unit.set(unit);
          }
        },
        error: (error: unknown) => {
          if (requestId === this.loadRequestId) {
            this.loadError.set(this.errorDetails(error));
          }
        },
      });
  }

  protected handleErrorAction(): void {
    if (this.loadError()?.notFound) {
      void this.router.navigate(['/unidades']);
      return;
    }

    this.loadUnit();
  }

  private errorDetails(error: unknown): DetailError {
    const problem = this.problemDetails(error);
    const trace = problem?.traceId ? ` Código de suporte: ${problem.traceId}.` : '';

    if (error instanceof HttpErrorResponse && error.status === 404) {
      return {
        description: `A unidade pode ter sido removida ou o endereço está incorreto.${trace}`,
        notFound: true,
        title: 'Unidade não encontrada',
      };
    }

    return {
      description: `Não foi possível consultar os dados da unidade. Tente novamente.${trace}`,
      notFound: false,
      title: 'Não foi possível carregar a unidade',
    };
  }

  private problemDetails(error: unknown): ProblemDetails | null {
    if (!(error instanceof HttpErrorResponse) || !error.error || typeof error.error !== 'object') {
      return null;
    }

    return error.error as ProblemDetails;
  }
}
