import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { User } from './user.models';
import { UsersApi } from './users-api';
import { UsersPage } from './users-page';

describe('UsersPage', () => {
  let fixture: ComponentFixture<UsersPage>;
  const api = {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
  };
  const user: User = {
    id: 'user-id',
    code: 'USR-01',
    login: 'maria',
    status: 1,
    createdAt: '2026-09-01T10:00:00Z',
    updatedAt: null,
  };

  beforeEach(async () => {
    vi.resetAllMocks();
    api.list.mockReturnValue(of([user]));
    await TestBed.configureTestingModule({
      imports: [UsersPage],
      providers: [{ provide: UsersApi, useValue: api }],
    }).compileComponents();
    fixture = TestBed.createComponent(UsersPage);
    fixture.detectChanges();
  });

  it('should load and filter users by code or login', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(api.list).toHaveBeenCalledWith(undefined);
    expect(element.querySelector('tbody')?.textContent).toContain('maria');

    const search = element.querySelector<HTMLInputElement>('#user-search');
    if (search) {
      search.value = 'inexistente';
      search.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();

    expect(element.textContent).toContain('Nenhum usuário encontrado');
  });

  it('should ignore a stale list response after changing the status filter', () => {
    fixture.destroy();
    const firstRequest = new Subject<readonly User[]>();
    const secondRequest = new Subject<readonly User[]>();
    const inactiveUser: User = { ...user, id: 'inactive-id', login: 'ana', status: 0 };
    api.list.mockReset();
    api.list.mockReturnValueOnce(firstRequest).mockReturnValueOnce(secondRequest);
    fixture = TestBed.createComponent(UsersPage);
    fixture.detectChanges();

    const status = (fixture.nativeElement as HTMLElement).querySelector<HTMLSelectElement>(
      '#status-filter',
    );
    if (status) {
      status.value = 'inactive';
      status.dispatchEvent(new Event('change'));
    }
    secondRequest.next([inactiveUser]);
    secondRequest.complete();
    firstRequest.next([user]);
    firstRequest.complete();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(api.list).toHaveBeenNthCalledWith(2, 0);
    expect(text).toContain('ana');
    expect(text).not.toContain('maria');
  });

  it('should expose the support trace when loading users fails', () => {
    fixture.destroy();
    api.list.mockReset();
    api.list.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            error: { detail: 'Unexpected error.', traceId: 'trace-list' },
            status: 500,
            statusText: 'Server Error',
          }),
      ),
    );

    fixture = TestBed.createComponent(UsersPage);
    fixture.detectChanges();

    const alert = (fixture.nativeElement as HTMLElement).querySelector('[role="alert"]');
    expect(alert?.textContent).toContain('Não foi possível carregar os usuários');
    expect(alert?.textContent).toContain('trace-list');
  });

  it('should create an inactive user', () => {
    api.create.mockReturnValue(of('new-user-id'));
    click('Novo usuário');
    fixture.componentInstance['createForm'].setValue({
      code: ' USR-02 ',
      login: ' joao ',
      password: 'password',
      status: 0,
    });

    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLFormElement>('form')
      ?.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(api.create).toHaveBeenCalledWith({
      code: 'USR-02',
      login: 'joao',
      password: 'password',
      status: 0,
    });
    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'Usuário criado com sucesso.',
    );
  });

  it('should reject a code containing only spaces before calling the API', () => {
    click('Novo usuário');
    fixture.componentInstance['createForm'].setValue({
      code: '   ',
      login: 'joao',
      password: 'password',
      status: 1,
    });

    submitEditor();

    expect(api.create).not.toHaveBeenCalled();
    expect(document.activeElement).toBe(
      (fixture.nativeElement as HTMLElement).querySelector('#create-code'),
    );
  });

  it('should prevent an empty user update', () => {
    click('Editar');
    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLFormElement>('form')
      ?.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(api.update).not.toHaveBeenCalled();
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('[role="alert"]')?.textContent,
    ).toContain('Altere a senha ou o status');
  });

  it('should update only the user password when status is unchanged', () => {
    api.update.mockReturnValue(of(undefined));
    click('Editar');
    fixture.componentInstance['editForm'].setValue({ password: 'new-password', status: 1 });

    submitEditor();

    expect(api.update).toHaveBeenCalledWith('user-id', { password: 'new-password' });
    expect((fixture.nativeElement as HTMLElement).querySelector('[role="dialog"]')).toBeNull();
    expect(api.list).toHaveBeenCalledTimes(2);
  });

  it('should update only the user status when password is blank', () => {
    api.update.mockReturnValue(of(undefined));
    click('Editar');
    fixture.componentInstance['editForm'].setValue({ password: '', status: 0 });

    submitEditor();

    expect(api.update).toHaveBeenCalledWith('user-id', { status: 0 });
  });

  it('should update password and status together', () => {
    api.update.mockReturnValue(of(undefined));
    click('Editar');
    fixture.componentInstance['editForm'].setValue({ password: 'new-password', status: 0 });

    submitEditor();

    expect(api.update).toHaveBeenCalledWith('user-id', {
      password: 'new-password',
      status: 0,
    });
  });

  it('should explain a missing user and expose the support trace', () => {
    api.update.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            error: { detail: 'User not found.', traceId: 'trace-404' },
            status: 404,
            statusText: 'Not Found',
          }),
      ),
    );
    click('Editar');
    fixture.componentInstance['editForm'].setValue({ password: '', status: 0 });

    submitEditor();
    fixture.detectChanges();

    const alert = (fixture.nativeElement as HTMLElement).querySelector('[role="alert"]');
    expect(alert?.textContent).toContain('Este usuário não existe mais');
    expect(alert?.textContent).toContain('trace-404');
  });

  it('should manage focus and close the editor with Escape', async () => {
    const element = fixture.nativeElement as HTMLElement;
    const opener = Array.from(element.querySelectorAll<HTMLButtonElement>('button')).find(
      (button) => button.textContent?.includes('Novo usuário'),
    );
    opener?.focus();
    opener?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(document.activeElement).toBe(element.querySelector('#create-code'));
    expect(element.querySelector('.page-content')?.hasAttribute('inert')).toBe(true);

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', cancelable: true }));
    fixture.detectChanges();

    expect(element.querySelector('[role="dialog"]')).toBeNull();
    expect(document.activeElement).toBe(opener);
  });

  it('should explain duplicate code or login conflicts', () => {
    api.create.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 409, statusText: 'Conflict' })),
    );
    click('Novo usuário');
    fixture.componentInstance['createForm'].setValue({
      code: 'USR-01',
      login: 'maria',
      password: 'password',
      status: 1,
    });

    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLFormElement>('form')
      ?.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(
      (fixture.nativeElement as HTMLElement).querySelector('[role="alert"]')?.textContent,
    ).toContain('Já existe um usuário');
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
