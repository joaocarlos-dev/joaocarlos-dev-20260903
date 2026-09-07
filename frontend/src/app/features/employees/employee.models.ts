export interface Employee {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly userId: string;
  readonly unitId: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
}

export interface CreateEmployeeRequest {
  readonly code: string;
  readonly name: string;
  readonly userId: string;
  readonly unitId: string;
}

export interface UpdateEmployeeRequest {
  readonly name?: string;
  readonly unitId?: string;
}
