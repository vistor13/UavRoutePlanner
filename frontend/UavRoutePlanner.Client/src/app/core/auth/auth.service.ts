import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { API_URL } from '../api-config';
import { TokenStorage } from './token-storage';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(TokenStorage);

  readonly isLoggedIn = signal(this.storage.get() !== null);
  readonly userName = signal<string | null>(null);

  get token(): string | null {
    return this.storage.get();
  }

  register(userName: string, password: string) {
    return this.http.post<void>(`${API_URL}/auth/register`, { userName, password });
  }

  login(userName: string, password: string) {
    return this.http
      .post<{ token: string }>(`${API_URL}/auth/login`, { userName, password })
      .pipe(
        tap(response => {
          this.storage.set(response.token);
          this.isLoggedIn.set(true);
        })
      );
  }

  loadProfile() {
    return this.http
      .get<{ userName: string }>(`${API_URL}/account/me`)
      .pipe(tap(profile => this.userName.set(profile.userName)));
  }

  changePassword(currentPassword: string, newPassword: string) {
    return this.http.put<void>(`${API_URL}/account/password`, { currentPassword, newPassword });
  }

  logout(): void {
    this.storage.clear();
    this.userName.set(null);
    this.isLoggedIn.set(false);
    void this.router.navigate(['/login']);
  }
}
