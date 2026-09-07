import { EntityStatus } from '../../core/api/api.models';
import { UserRole } from '../../core/auth/auth.models';

export type { EntityStatus, ProblemDetails } from '../../core/api/api.models';

export interface User {
  readonly id: string;
  readonly code: string;
  readonly login: string;
  readonly status: EntityStatus;
  readonly role: UserRole;
  readonly createdAt: string;
  readonly updatedAt: string | null;
}

export interface CreateUserRequest {
  readonly code: string;
  readonly login: string;
  readonly password: string;
  readonly status: EntityStatus;
}

export interface UpdateUserRequest {
  readonly password?: string;
  readonly status?: EntityStatus;
}
