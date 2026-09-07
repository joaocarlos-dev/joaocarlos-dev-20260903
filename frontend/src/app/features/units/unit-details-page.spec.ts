import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { Unit } from './unit.models';
import { UnitDetailsPage } from './unit-details-page';
import { UnitsApi } from './units-api';

describe('UnitDetailsPage', () => {
  let fixture: ComponentFixture<UnitDetailsPage>;
  let router: Router;
  const params = new BehaviorSubject(convertToParamMap({ id: 'unit-id' }));
  const api = {
    get: vi.fn(),
  };
  const unit: Unit = {
    id: 'unit-id',
    code: 'UNIT-01',
    name: 'Matriz',
    status: 1,
    employees: [
      {
        id: 'employee-id',
        code: 'EMP-01',
        name: 'Maria Silva',
        userId: 'user-id',
        unitId: 'unit-id',
        createdAt: '2026-09-01T10:00:00Z',
        updatedAt: null,
      },
    ],
    createdAt: '2026-08-01T10:00:00Z',
    updatedAt: '2026-09-02T11:00:00Z',
  };

  beforeEach(async () => {
    vi.resetAllMocks();
    params.next(convertToParamMap({ id: 'unit-id' }));
    api.get.mockReturnValue(of(unit));
    await TestBed.configureTestingModule({
      imports: [UnitDetailsPage],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: params.asObservable() } },
        { provide: UnitsApi, useValue: api },
      ],
    }).compileComponents();
    router = TestBed.inject(Router);
  });

  it('should load the route unit and show its metadata and employees', () => {
    createComponent();

    const element = fixture.nativeElement as HTMLElement;
    expect(api.get).toHaveBeenCalledWith('unit-id');
    expect(element.textContent).toContain('Matriz');
    expect(element.textContent).toContain('UNIT-01');
    expect(element.textContent).toContain('Ativa');
    expect(element.textContent).toContain('Maria Silva');
    expect(element.textContent).toContain('EMP-01');
    expect(element.querySelector('caption')?.textContent).toContain('Matriz');
  });

  it('should show loading while the unit request is pending', () => {
    const request = new Subject<Unit>();
    api.get.mockReturnValue(request);
    createComponent();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Carregando unidade');

    request.next(unit);
    request.complete();
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Matriz');
  });

  it('should show an empty state when the unit has no employees', () => {
    api.get.mockReturnValue(of({ ...unit, employees: [] }));
    createComponent();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Nenhum colaborador vinculado');
    expect(element.querySelector('table')).toBeNull();
  });

  it('should show a specific not-found state with trace id and return to units', () => {
    api.get.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            error: { traceId: 'trace-404' },
            status: 404,
            statusText: 'Not Found',
          }),
      ),
    );
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    createComponent();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Unidade não encontrada');
    expect(element.textContent).toContain('trace-404');

    clickButton('Voltar para unidades');
    expect(navigate).toHaveBeenCalledWith(['/unidades']);
  });

  it('should expose the support trace and retry a generic load failure', () => {
    api.get
      .mockReturnValueOnce(
        throwError(
          () =>
            new HttpErrorResponse({
              error: { traceId: 'trace-500' },
              status: 500,
              statusText: 'Server Error',
            }),
        ),
      )
      .mockReturnValueOnce(of(unit));
    createComponent();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Não foi possível carregar a unidade');
    expect(element.textContent).toContain('trace-500');

    clickButton('Tentar novamente');
    expect(api.get).toHaveBeenCalledTimes(2);
    expect(element.textContent).toContain('Matriz');
  });

  it('should reload when the route id changes and ignore a stale response', () => {
    const firstRequest = new Subject<Unit>();
    api.get.mockReturnValueOnce(firstRequest).mockReturnValueOnce(of({ ...unit, id: 'unit-2', name: 'Filial' }));
    createComponent();

    params.next(convertToParamMap({ id: 'unit-2' }));
    fixture.detectChanges();
    firstRequest.next(unit);
    firstRequest.complete();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(api.get).toHaveBeenNthCalledWith(2, 'unit-2');
    expect(text).toContain('Filial');
    expect(text).not.toContain('Matriz');
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(UnitDetailsPage);
    fixture.detectChanges();
  }

  function clickButton(label: string): void {
    const buttons = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLButtonElement>('button'),
    );
    buttons.find((button) => button.textContent?.includes(label))?.click();
    fixture.detectChanges();
  }
});
