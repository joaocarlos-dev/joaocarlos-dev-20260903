export interface LoginRequest {
  readonly login: string;
  readonly password: string;
}

export interface AuthenticationResponse {
  readonly accessToken: string;
}

export interface SessionIdentity {
  readonly login: string;
  readonly expiresAt: number;
}
