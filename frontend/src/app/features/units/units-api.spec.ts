import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UnitsApi } from './units-api';

describe('UnitsApi', () => {
  let api: UnitsApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(UnitsApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should list and get units through the API contract', () => {
    api.list().subscribe();
    const list = http.expectOne('/api/v1/units');
    expect(list.request.method).toBe('GET');
    list.flush([]);

    api.get('unit-id').subscribe();
    const detail = http.expectOne('/api/v1/units/unit-id');
    expect(detail.request.method).toBe('GET');
    detail.flush({});
  });

  it('should create and update units through the API contract', () => {
    api.create({ code: 'UNIT-01', name: 'Matriz' }).subscribe();
    const creation = http.expectOne('/api/v1/units');
    expect(creation.request.method).toBe('POST');
    expect(creation.request.body).toEqual({ code: 'UNIT-01', name: 'Matriz' });
    creation.flush('unit-id');

    api.update('unit-id', { name: 'Nova matriz', status: 0 }).subscribe();
    const update = http.expectOne('/api/v1/units/unit-id');
    expect(update.request.method).toBe('PATCH');
    expect(update.request.body).toEqual({ name: 'Nova matriz', status: 0 });
    update.flush(null);
  });
});
