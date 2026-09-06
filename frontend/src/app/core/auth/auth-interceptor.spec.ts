import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthApi } from './auth-api';
import { authInterceptor } from './auth-interceptor';
import { AuthSession } from './auth-session';

describe('authInterceptor', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('should add the bearer token to protected API requests', () => {
    const session = TestBed.inject(AuthSession);
    const http = TestBed.inject(HttpClient);
    session.start(createToken());

    http.get('/api/v1/users').subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/v1/users');

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${session.token()}`);
    request.flush([]);
  });

  it('should clear the session and navigate to login after a protected 401', () => {
    const session = TestBed.inject(AuthSession);
    const http = TestBed.inject(HttpClient);
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    session.start(createToken());

    http.get('/api/v1/users').subscribe({ error: () => undefined });
    const request = TestBed.inject(HttpTestingController).expectOne('/api/v1/users');
    request.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(session.authenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { expired: 1, returnUrl: '/' },
    });
  });

  it('should not treat invalid login as an expired session', () => {
    const session = TestBed.inject(AuthSession);
    const api = TestBed.inject(AuthApi);
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate');
    session.start(createToken());

    api.login({ login: 'admin', password: 'password' }).subscribe({ error: () => undefined });
    const request = TestBed.inject(HttpTestingController).expectOne('/api/v1/auth/login');

    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(session.authenticated()).toBe(true);
    expect(navigate).not.toHaveBeenCalled();
  });
});

function createToken(): string {
  const expiresAt = Math.floor(Date.now() / 1000) + 3600;
  const encode = (value: object) =>
    btoa(JSON.stringify(value)).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_');
  return `${encode({ alg: 'none' })}.${encode({ exp: expiresAt, unique_name: 'admin' })}.signature`;
}
