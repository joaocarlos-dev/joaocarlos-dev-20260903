import { EntityStatus } from '../../core/api/api.models';

export interface UnitEmployee {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly userId: string;
  readonly unitId: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
}

export interface Unit {
  readonly id: string;
  readonly code: string;
  readonly name: string;
  readonly status: EntityStatus;
  readonly employees: readonly UnitEmployee[];
  readonly createdAt: string;
  readonly updatedAt: string | null;
}

export interface CreateUnitRequest {
  readonly code: string;
  readonly name: string;
}

export interface UpdateUnitRequest {
  readonly name?: string;
  readonly status?: EntityStatus;
}
