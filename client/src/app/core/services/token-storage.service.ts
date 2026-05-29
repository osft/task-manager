import { Injectable } from '@angular/core';

const TOKEN_KEY = 'tm_jwt';
const EXPIRES_KEY = 'tm_expires';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getExpiresAt(): string | null {
    return localStorage.getItem(EXPIRES_KEY);
  }

  save(token: string, expiresAt?: string | null): void {
    localStorage.setItem(TOKEN_KEY, token);
    if (expiresAt) {
      localStorage.setItem(EXPIRES_KEY, expiresAt);
    } else {
      localStorage.removeItem(EXPIRES_KEY);
    }
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_KEY);
  }

  hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    const expires = this.getExpiresAt();
    if (!expires) {
      return true;
    }

    return new Date(expires).getTime() > Date.now();
  }
}
