import { DOCUMENT } from '@angular/common';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { SessionIdentity, UserRole } from './auth.models';

interface JwtPayload {
  readonly exp?: unknown;
  readonly unique_name?: unknown;
  readonly role?: unknown;
  readonly ['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']?: unknown;
}

@Injectable({ providedIn: 'root' })
export class AuthSession {
  private readonly storageKey = 'employee-management.access-token';
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);
  private readonly tokenState = signal<string | null>(this.readStoredToken());
  private readonly identityState = computed(() => this.decode(this.tokenState()));
  private expiryTimer: ReturnType<typeof setTimeout> | null = null;

  readonly identity = () => this.validIdentity();
  readonly authenticated = () => this.validIdentity() !== null;

  constructor() {
    const identity = this.identityState();
    if (identity) {
      this.scheduleExpiry(identity);
    }
  }

  token(): string | null {
    return this.authenticated() ? this.tokenState() : null;
  }

  start(accessToken: string): boolean {
    const identity = this.decode(accessToken);
    if (!identity || this.isExpired(identity)) {
      this.clear();
      return false;
    }

    this.tokenState.set(accessToken);
    this.storage()?.setItem(this.storageKey, accessToken);
    this.scheduleExpiry(identity);
    return true;
  }

  clear(): void {
    if (this.expiryTimer) {
      clearTimeout(this.expiryTimer);
      this.expiryTimer = null;
    }
    this.tokenState.set(null);
    this.storage()?.removeItem(this.storageKey);
  }

  private validIdentity(): SessionIdentity | null {
    const identity = this.identityState();
    if (!identity || this.isExpired(identity)) {
      if (identity) {
        this.clear();
      }
      return null;
    }

    return identity;
  }

  private isExpired(identity: SessionIdentity): boolean {
    return identity.expiresAt <= Date.now();
  }

  private scheduleExpiry(identity: SessionIdentity): void {
    if (this.expiryTimer) {
      clearTimeout(this.expiryTimer);
    }

    const delay = Math.max(0, Math.min(identity.expiresAt - Date.now(), 2_147_483_647));
    this.expiryTimer = setTimeout(() => {
      if (this.isExpired(identity)) {
        this.expireSession();
      } else {
        this.scheduleExpiry(identity);
      }
    }, delay);
  }

  private expireSession(): void {
    const returnUrl = this.router.url !== '/login' ? this.router.url : undefined;
    this.clear();
    void this.router.navigate(['/login'], { queryParams: { expired: 1, returnUrl } });
  }

  private readStoredToken(): string | null {
    const token = this.storage()?.getItem(this.storageKey) ?? null;
    if (!token) {
      return null;
    }

    const identity = this.decode(token);
    if (!identity || this.isExpired(identity)) {
      this.storage()?.removeItem(this.storageKey);
      return null;
    }

    return token;
  }

  private decode(token: string | null): SessionIdentity | null {
    if (!token) {
      return null;
    }

    try {
      const segments = token.split('.');
      if (segments.length !== 3) {
        return null;
      }

      const payload = JSON.parse(this.decodeBase64Url(segments[1])) as JwtPayload;
      if (typeof payload.exp !== 'number' || typeof payload.unique_name !== 'string') {
        return null;
      }

      const roleValue = payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      const role = roleValue === 'Administrator' || roleValue === 'Conventional'
        ? roleValue as UserRole
        : null;
      return {
        expiresAt: payload.exp * 1000,
        login: payload.unique_name,
        role,
        isAdministrator: role === 'Administrator',
      };
    } catch {
      return null;
    }
  }

  private decodeBase64Url(value: string): string {
    const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (normalized.length % 4)) % 4);
    const binary = this.document.defaultView?.atob(normalized + padding) ?? '';
    const bytes = Uint8Array.from(binary, (character) => character.charCodeAt(0));
    return new TextDecoder().decode(bytes);
  }

  private storage(): Storage | null {
    try {
      return this.document.defaultView?.sessionStorage ?? null;
    } catch {
      return null;
    }
  }
}
