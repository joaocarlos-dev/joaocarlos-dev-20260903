import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthSession } from './auth-session';

describe('AuthSession', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  afterEach(() => vi.useRealTimers());

  it('should keep a valid token only in session storage', () => {
    const session = TestBed.inject(AuthSession);
    const token = createToken('admin', Math.floor(Date.now() / 1000) + 3600);

    expect(session.start(token)).toBe(true);
    expect(session.authenticated()).toBe(true);
    expect(session.identity()?.login).toBe('admin');
    expect(sessionStorage.length).toBe(1);
    expect(localStorage.length).toBe(0);
  });

  it('should expose the administrator role from the token', () => {
    const session = TestBed.inject(AuthSession);
    const token = createToken('admin', Math.floor(Date.now() / 1000) + 3600, 'Administrator');

    expect(session.start(token)).toBe(true);
    expect(session.identity()?.role).toBe('Administrator');
    expect(session.identity()?.isAdministrator).toBe(true);
  });

  it('should expose a conventional role and treat unknown roles as non-administrative', () => {
    const session = TestBed.inject(AuthSession);

    expect(session.start(createToken('user', Math.floor(Date.now() / 1000) + 3600, 'Conventional'))).toBe(true);
    expect(session.identity()?.role).toBe('Conventional');
    expect(session.identity()?.isAdministrator).toBe(false);

    session.clear();
    expect(session.start(createToken('user', Math.floor(Date.now() / 1000) + 3600, 'Unknown'))).toBe(true);
    expect(session.identity()?.role).toBeNull();
    expect(session.identity()?.isAdministrator).toBe(false);
  });

  it('should read the Microsoft role claim format', () => {
    const session = TestBed.inject(AuthSession);
    const token = createToken(
      'admin',
      Math.floor(Date.now() / 1000) + 3600,
      'Administrator',
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
    );

    expect(session.start(token)).toBe(true);
    expect(session.identity()?.isAdministrator).toBe(true);
  });

  it('should reject and remove expired tokens', () => {
    const session = TestBed.inject(AuthSession);
    const token = createToken('admin', Math.floor(Date.now() / 1000) - 1);

    expect(session.start(token)).toBe(false);
    expect(session.authenticated()).toBe(false);
    expect(session.token()).toBeNull();
    expect(sessionStorage.length).toBe(0);
  });

  it('should reject malformed tokens', () => {
    const session = TestBed.inject(AuthSession);

    expect(session.start('not-a-jwt')).toBe(false);
    expect(session.authenticated()).toBe(false);
  });

  it('should clear the current session', () => {
    const session = TestBed.inject(AuthSession);
    session.start(createToken('admin', Math.floor(Date.now() / 1000) + 3600));

    session.clear();

    expect(session.authenticated()).toBe(false);
    expect(sessionStorage.length).toBe(0);
  });

  it('should clear and redirect when an active session expires', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-09-06T12:00:00Z'));
    const session = TestBed.inject(AuthSession);
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const expiresAt = Math.floor(Date.now() / 1000) + 2;
    session.start(createToken('admin', expiresAt));

    vi.advanceTimersByTime(2000);

    expect(session.authenticated()).toBe(false);
    expect(sessionStorage.length).toBe(0);
    expect(navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { expired: 1, returnUrl: '/' },
    });
  });
});

function createToken(login: string, expiresAt: number, role?: string, roleClaim = 'role'): string {
  const encode = (value: object) =>
    btoa(JSON.stringify(value)).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode({ alg: 'none' })}.${encode({ exp: expiresAt, unique_name: login, ...(role ? { [roleClaim]: role } : {}) })}.signature`;
}
