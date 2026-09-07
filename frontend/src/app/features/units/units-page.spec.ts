import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Unit } from './unit.models';
import { UnitsApi } from './units-api';
import { UnitsPage } from './units-page';
import { AuthSession } from '../../core/auth/auth-session';

describe('UnitsPage', () => {
  let fixture: ComponentFixture<UnitsPage>;
  const api = { list: vi.fn(), create: vi.fn(), get: vi.fn(), update: vi.fn() };
  const administrator = signal(true);
  const session = { identity: () => ({ isAdministrator: administrator() }) };
  const unit: Unit = {
    id: 'unit-id',
    code: 'UNIT-01',
    name: 'Matriz',
    status: 1,
    employees: [],
    createdAt: '2026-09-01T10:00:00Z',
    updatedAt: null,
  };

  beforeEach(async () => {
    vi.resetAllMocks();
    administrator.set(true);
    api.list.mockReturnValue(of([unit]));
    await TestBed.configureTestingModule({
      imports: [UnitsPage],
      providers: [provideRouter([]), { provide: UnitsApi, useValue: api }, { provide: AuthSession, useValue: session }],
    }).compileComponents();
    fixture = TestBed.createComponent(UnitsPage);
    fixture.detectChanges();
  });

  it('should load and filter units locally by code, name and status', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(api.list).toHaveBeenCalledOnce();
    expect(element.textContent).toContain('Matriz');
    const search = element.querySelector<HTMLInputElement>('#unit-search');
    if (search) {
      search.value = 'inexistente';
      search.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();
    expect(element.textContent).toContain('Nenhuma unidade encontrada');
  });

  it('should hide and block unit mutations for conventional sessions', () => {
    administrator.set(false);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.page-header .button--primary')).toBeNull();
    expect(element.querySelector('.edit-button')).toBeNull();
    fixture.componentInstance['openCreate']();
    fixture.componentInstance['openEdit'](unit);
    fixture.componentInstance['submitCreate']();
    fixture.componentInstance['submitEdit']();
    expect(fixture.componentInstance['editorMode']()).toBeNull();
    expect(api.create).not.toHaveBeenCalled();
    expect(api.update).not.toHaveBeenCalled();
  });

  it('should point the details action to the selected unit route', () => {
    const detailsLink = (fixture.nativeElement as HTMLElement).querySelector<HTMLAnchorElement>(
      '.details-button',
    );
    expect(detailsLink?.getAttribute('href')).toBe('/unidades/unit-id');
  });

  it('should create a unit with trimmed values', () => {
    api.create.mockReturnValue(of('new-unit-id'));
    click('Nova unidade');
    fixture.componentInstance['createForm'].setValue({ code: ' UNIT-02 ', name: ' Filial ' });
    submitEditor();
    expect(api.create).toHaveBeenCalledWith({ code: 'UNIT-02', name: 'Filial' });
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Unidade criada com sucesso.');
  });

  it('should dismiss the success feedback after two seconds', () => {
    vi.useFakeTimers();
    try {
      api.create.mockReturnValue(of('new-unit-id'));
      click('Nova unidade');
      fixture.componentInstance['createForm'].setValue({ code: 'UNIT-02', name: 'Filial' });
      submitEditor();
      expect(fixture.componentInstance['feedback']()).toBe('Unidade criada com sucesso.');
      vi.advanceTimersByTime(1999);
      expect(fixture.componentInstance['feedback']()).toBe('Unidade criada com sucesso.');
      vi.advanceTimersByTime(1);
      expect(fixture.componentInstance['feedback']()).toBeNull();
    } finally {
      vi.useRealTimers();
    }
  });

  it('should reject a name containing only spaces before calling the API', () => {
    click('Nova unidade');
    fixture.componentInstance['createForm'].setValue({ code: 'UNIT-02', name: '   ' });
    submitEditor();
    expect(api.create).not.toHaveBeenCalled();
    expect(document.activeElement).toBe(
      (fixture.nativeElement as HTMLElement).querySelector('#create-name'),
    );
  });

  it('should prevent an empty unit update', () => {
    click('Editar');
    submitEditor();
    expect(api.update).not.toHaveBeenCalled();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Altere o nome ou o status',
    );
  });

  it('should send only changed fields when updating a unit', () => {
    api.update.mockReturnValue(of(undefined));
    click('Editar');
    fixture.componentInstance['editForm'].setValue({ name: 'Nova matriz', status: 1 });
    submitEditor();
    expect(api.update).toHaveBeenCalledWith('unit-id', { name: 'Nova matriz' });
  });

  it('should explain duplicate codes and missing units with trace ids', () => {
    api.create.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            error: { traceId: 'trace-409' },
            status: 409,
            statusText: 'Conflict',
          }),
      ),
    );
    click('Nova unidade');
    fixture.componentInstance['createForm'].setValue({ code: 'UNIT-01', name: 'Outra' });
    submitEditor();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Já existe uma unidade');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('trace-409');

    api.update.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            error: { traceId: 'trace-404' },
            status: 404,
            statusText: 'Not Found',
          }),
      ),
    );
    click('Fechar');
    click('Editar');
    fixture.componentInstance['editForm'].setValue({ name: 'Outra matriz', status: 1 });
    submitEditor();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Esta unidade não existe mais');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('trace-404');
  });

  function click(label: string): void {
    const buttons = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLButtonElement>('button'),
    );
    buttons.find((button) => button.textContent?.includes(label))?.click();
    fixture.detectChanges();
  }

  function submitEditor(): void {
    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLFormElement>('form')
      ?.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }
});
