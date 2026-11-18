import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../api-config';
import { UavProfile, UavProfileInput } from '../models';

@Injectable({ providedIn: 'root' })
export class UavProfileApi {
  private readonly http = inject(HttpClient);
  private readonly url = `${API_URL}/uav-profiles`;

  getAll() {
    return this.http.get<UavProfile[]>(this.url);
  }

  create(input: UavProfileInput) {
    return this.http.post<UavProfile>(this.url, input);
  }

  update(id: number, input: UavProfileInput) {
    return this.http.put<UavProfile>(`${this.url}/${id}`, input);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  getPhoto(id: number) {
    return this.http.get(`${this.url}/${id}/photo`, { responseType: 'blob' });
  }

  uploadPhoto(id: number, photo: Blob) {
    return this.http.put<UavProfile>(`${this.url}/${id}/photo`, photo, {
      headers: { 'Content-Type': photo.type }
    });
  }

  deletePhoto(id: number) {
    return this.http.delete<UavProfile>(`${this.url}/${id}/photo`);
  }
}
