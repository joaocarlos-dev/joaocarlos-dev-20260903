export interface LoginRequest {
  readonly login: string;
  readonly password: string;
}

export interface AuthenticationResponse {
  readonly accessToken: string;
}

export type UserRole = 'Conventional' | 'Administrator';

export interface SessionIdentity {
  readonly login: string;
  readonly expiresAt: number;
  readonly role: UserRole | null;
  readonly isAdministrator: boolean;
}
