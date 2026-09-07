import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { EmployeesApi } from './employees-api';

describe('EmployeesApi', () => {
  let api: EmployeesApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(EmployeesApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should use the employees CRUD contract', () => {
    api.list().subscribe();
    const request = http.expectOne('/api/v1/employees');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });
});
