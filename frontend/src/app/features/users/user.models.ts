export type EntityStatus = 0 | 1;

export interface User {
  readonly id: string;
  readonly code: string;
  readonly login: string;
  readonly status: EntityStatus;
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

export interface ProblemDetails {
  readonly detail?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
  readonly title?: string;
  readonly traceId?: string;
}
