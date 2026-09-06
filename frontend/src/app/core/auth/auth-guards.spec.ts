import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { authGuard, guestGuard } from './auth-guards';
import { AuthSession } from './auth-session';

describe('authentication guards', () => {
  const session = { authenticated: vi.fn() };

  beforeEach(() => {
    session.authenticated.mockReset();
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthSession, useValue: session },
        {
          provide: Router,
          useValue: {
            createUrlTree: vi.fn((commands: string[], options?: object) => ({ commands, options })),
          },
        },
      ],
    });
  });

  it('should preserve the intended route when authentication is required', () => {
    session.authenticated.mockReturnValue(false);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/usuarios' } as RouterStateSnapshot),
    ) as UrlTree;
    const router = TestBed.inject(Router);

    expect(router.createUrlTree).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/usuarios' },
    });
    expect(result).toBeTruthy();
  });

  it('should open a protected route for an authenticated session', () => {
    session.authenticated.mockReturnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/dashboard' } as RouterStateSnapshot),
    );

    expect(result).toBe(true);
  });

  it('should redirect an authenticated visitor away from login', () => {
    session.authenticated.mockReturnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      guestGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as UrlTree;
    const router = TestBed.inject(Router);

    expect(router.createUrlTree).toHaveBeenCalledWith(['/dashboard']);
    expect(result).toBeTruthy();
  });
});
