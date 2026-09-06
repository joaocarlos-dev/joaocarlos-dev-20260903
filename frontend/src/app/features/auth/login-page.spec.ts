import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthApi } from '../../core/auth/auth-api';
import { AuthSession } from '../../core/auth/auth-session';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  const api = { login: vi.fn() };
  const session = { start: vi.fn() };
  const router = { navigateByUrl: vi.fn() };

  beforeEach(async () => {
    vi.resetAllMocks();
    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        { provide: AuthApi, useValue: api },
        { provide: AuthSession, useValue: session },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: { get: (key: string) => (key === 'returnUrl' ? '/usuarios' : null) },
            },
          },
        },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();
  });

  it('should validate required credentials before calling the API', () => {
    submitForm();

    expect(api.login).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelectorAll('.field__error')).toHaveLength(2);
  });

  it('should start the session and return to the intended route', () => {
    api.login.mockReturnValue(of({ accessToken: 'valid-token' }));
    session.start.mockReturnValue(true);
    fillCredentials();

    submitForm();

    expect(api.login).toHaveBeenCalledWith({ login: 'admin', password: 'password' });
    expect(session.start).toHaveBeenCalledWith('valid-token');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/usuarios');
  });

  it('should show a specific message for invalid credentials', () => {
    api.login.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' })),
    );
    fillCredentials();

    submitForm();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Login ou senha inválidos.',
    );
  });

  function fillCredentials(): void {
    const element = fixture.nativeElement as HTMLElement;
    const login = element.querySelector<HTMLInputElement>('#login');
    const password = element.querySelector<HTMLInputElement>('#password');
    if (login && password) {
      login.value = 'admin';
      login.dispatchEvent(new Event('input'));
      password.value = 'password';
      password.dispatchEvent(new Event('input'));
    }
  }

  function submitForm(): void {
    (fixture.nativeElement as HTMLElement)
      .querySelector<HTMLFormElement>('form')
      ?.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
  }
});
