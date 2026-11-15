import { Injectable } from '@angular/core';

const TOKEN_KEY = 'uav_token';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  private memoryToken: string | null = null;

  get(): string | null {
    if (this.memoryToken) return this.memoryToken;
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  }

  set(token: string): void {
    this.memoryToken = token;
    try {
      localStorage.setItem(TOKEN_KEY, token);
    } catch {
      return;
    }
  }

  clear(): void {
    this.memoryToken = null;
    try {
      localStorage.removeItem(TOKEN_KEY);
    } catch {
      return;
    }
  }
}
