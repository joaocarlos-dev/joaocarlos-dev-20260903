import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { throwError } from 'rxjs';
import { DashboardPage } from './dashboard-page';
import { EmployeesApi } from '../employees/employees-api';
import { UnitsApi } from '../units/units-api';
import { UsersApi } from '../users/users-api';

describe('DashboardPage', () => {
  let fixture: ComponentFixture<DashboardPage>;
  const employeesApi = { list: vi.fn() };
  const unitsApi = { list: vi.fn() };
  const usersApi = { list: vi.fn() };

  beforeEach(async () => {
    vi.resetAllMocks();
    employeesApi.list.mockReturnValue(of([{}, {}]));
    unitsApi.list.mockReturnValue(of([{}]));
    usersApi.list.mockReturnValue(of([{}, {}, {}]));
    await TestBed.configureTestingModule({
      imports: [DashboardPage],
      providers: [provideRouter([]), { provide: EmployeesApi, useValue: employeesApi }, { provide: UnitsApi, useValue: unitsApi }, { provide: UsersApi, useValue: usersApi }],
    }).compileComponents();
    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
  });

  it('should show totals derived from all resource listings', () => {
    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('3');
    expect(text).toContain('1');
    expect(text).toContain('2');
  });

  it('should show the API detail and trace id and retry successfully', () => {
    employeesApi.list.mockReturnValueOnce(throwError(() => new HttpErrorResponse({ status: 500, error: { detail: 'Falha temporária', traceId: 'trace-dashboard' } })));
    fixture.componentInstance['loadTotals']();
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('trace-dashboard');

    employeesApi.list.mockReturnValue(of([{}]));
    fixture.componentInstance['loadTotals']();
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Resumo operacional');
    expect(fixture.componentInstance['error']()).toBeNull();
  });
});
