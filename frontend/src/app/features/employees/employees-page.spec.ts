import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { User } from '../users/user.models';
import { Unit } from '../units/unit.models';
import { Employee } from './employee.models';
import { EmployeesApi } from './employees-api';
import { EmployeesPage } from './employees-page';
import { UnitsApi } from '../units/units-api';
import { UsersApi } from '../users/users-api';

describe('EmployeesPage', () => {
  let fixture: ComponentFixture<EmployeesPage>;
  const employee: Employee = { id: 'employee-id', code: 'EMP-01', name: 'Ana', userId: 'user-id', unitId: 'unit-id', createdAt: '2026-09-01T10:00:00Z', updatedAt: null };
  const user: User = { id: 'user-id', code: 'USR-01', login: 'ana', status: 1, createdAt: '2026-09-01T10:00:00Z', updatedAt: null };
  const unit: Unit = { id: 'unit-id', code: 'UNIT-01', name: 'Matriz', status: 1, employees: [], createdAt: '2026-09-01T10:00:00Z', updatedAt: null };
  const employeesApi = { list: vi.fn(), get: vi.fn(), create: vi.fn(), update: vi.fn(), delete: vi.fn() };
  const usersApi = { list: vi.fn() };
  const unitsApi = { list: vi.fn() };

  beforeEach(async () => {
    vi.resetAllMocks();
    employeesApi.list.mockReturnValue(of([employee]));
    usersApi.list.mockReturnValue(of([user]));
    unitsApi.list.mockReturnValue(of([unit]));
    await TestBed.configureTestingModule({
      imports: [EmployeesPage],
      providers: [provideRouter([]), { provide: EmployeesApi, useValue: employeesApi }, { provide: UsersApi, useValue: usersApi }, { provide: UnitsApi, useValue: unitsApi }],
    }).compileComponents();
    fixture = TestBed.createComponent(EmployeesPage);
    fixture.detectChanges();
  });

  it('should resolve related user and unit names', () => {
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('ana');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Matriz');
  });

  it('should create a trimmed employee payload', () => {
    employeesApi.create.mockReturnValue(of('new-id'));
    fixture.componentInstance['openCreate']();
    fixture.componentInstance['createForm'].setValue({ code: ' EMP-02 ', name: ' Bruno ', userId: 'new-user', unitId: 'unit-id' });
    fixture.componentInstance['submitCreate']();
    expect(employeesApi.create).toHaveBeenCalledWith({ code: 'EMP-02', name: 'Bruno', userId: 'new-user', unitId: 'unit-id' });
  });
});
