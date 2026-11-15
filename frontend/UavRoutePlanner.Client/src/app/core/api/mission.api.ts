import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../api-config';
import { Mission, MissionInput, MissionSummary } from '../models';

@Injectable({ providedIn: 'root' })
export class MissionApi {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/missions`;

  getAll() {
    return this.http.get<MissionSummary[]>(this.url);
  }

  getById(id: number) {
    return this.http.get<Mission>(`${this.url}/${id}`);
  }

  create(input: MissionInput) {
    return this.http.post<Mission>(this.url, input);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
