import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
  UserProfile
} from '../models/auth.model';
import { ApiErrorBody, readApiError } from '../models/api-error.model';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly tokens = inject(TokenStorageService);

  private readonly baseUrl = environment.apiBaseUrl;
  readonly profile = signal<UserProfile | null>(null);

  isAuthenticated(): boolean {
    return this.tokens.hasValidToken();
  }

  login(request: LoginRequest): Observable<void> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/api/auth/login`, request).pipe(
      tap((response) => this.handleAuthResponse(response)),
      map(() => void 0),
      catchError((err) => throwError(() => this.extractError(err)))
    );
  }

  register(request: RegisterRequest): Observable<string> {
    return this.http
      .post<RegisterResponse>(`${this.baseUrl}/api/auth/register`, request)
      .pipe(
        map((r) => r.userId),
        catchError((err) => throwError(() => this.extractError(err)))
      );
  }

  loadProfile(): Observable<UserProfile | null> {
    if (!this.isAuthenticated()) {
      this.profile.set(null);
      return of(null);
    }

    return this.http.get<UserProfile>(`${this.baseUrl}/api/me`).pipe(
      tap((p) => this.profile.set(p)),
      catchError(() => {
        this.logout(false);
        return of(null);
      })
    );
  }

  logout(navigate = true): void {
    this.tokens.clear();
    this.profile.set(null);
    if (navigate) {
      void this.router.navigate(['/login']);
    }
  }

  private handleAuthResponse(response: AuthResponse): void {
    if (response.requiresMfa) {
      throw new Error('Multi-factor authentication is required but not yet supported in the UI.');
    }

    if (!response.token) {
      throw new Error('No token returned from the server.');
    }

    this.tokens.save(response.token, response.expiresAt ?? null);
  }

  private extractError(err: { status?: number; error?: ApiErrorBody }): string {
    if (err?.error) {
      return readApiError(err.error);
    }

    if (err?.status === 401) {
      return 'Invalid email or password.';
    }

    if (err?.status === 409) {
      return 'An account with this email already exists.';
    }

    return 'Authentication failed. Please try again.';
  }
}
