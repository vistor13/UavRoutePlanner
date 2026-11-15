import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../api-config';
import { ImportResult } from '../models';

@Injectable({ providedIn: 'root' })
export class ImportApi {
  private readonly http = inject(HttpClient);

  importObjects(fileName: string, content: string) {
    return this.http.post<ImportResult>(`${API_URL}/objects/import`, { fileName, content });
  }
}
