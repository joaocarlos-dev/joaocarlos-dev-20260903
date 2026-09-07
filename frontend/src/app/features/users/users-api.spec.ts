import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UsersApi } from './users-api';

describe('UsersApi', () => {
  let api: UsersApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(UsersApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should list users with an optional status filter', () => {
    api.list(1).subscribe();

    const request = http.expectOne('/api/v1/users?status=Active');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('should create and update users through the API contract', () => {
    api.create({ code: 'USR-01', login: 'maria', password: 'password', status: 1 }).subscribe();
    const creation = http.expectOne('/api/v1/users');
    expect(creation.request.method).toBe('POST');
    expect(creation.request.body).toEqual({
      code: 'USR-01',
      login: 'maria',
      password: 'password',
      status: 1,
    });
    creation.flush('user-id');

    api.update('user-id', { status: 0 }).subscribe();
    const update = http.expectOne('/api/v1/users/user-id');
    expect(update.request.method).toBe('PATCH');
    expect(update.request.body).toEqual({ status: 0 });
    update.flush(null);
  });
});
