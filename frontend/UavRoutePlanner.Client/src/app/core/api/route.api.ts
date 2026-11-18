import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../api-config';
import {
  DepartureRecommendation, ExportFormat, Mission, RouteComparison, RouteRequest, RouteResult
} from '../models';

@Injectable({ providedIn: 'root' })
export class RouteApi {
  private readonly http = inject(HttpClient);

  private url(missionId: number): string {
    return `${API_URL}/missions/${missionId}/routes`;
  }

  getAll(missionId: number) {
    return this.http.get<RouteResult[]>(this.url(missionId));
  }

  build(missionId: number, request: RouteRequest) {
    return this.http.post<RouteResult>(this.url(missionId), request);
  }

  compare(missionId: number, request: RouteRequest) {
    return this.http.post<RouteComparison>(`${this.url(missionId)}/compare`, request);
  }

  findBestDeparture(missionId: number, fromUtc: string, toUtc: string, seed: number | null) {
    return this.http.post<DepartureRecommendation>(`${this.url(missionId)}/best-departure`, { fromUtc, toUtc, seed });
  }

  saveDeparturePlan(missionId: number, name: string, departureTimeUtc: string, seed: number) {
    return this.http.post<Mission>(`${this.url(missionId)}/best-departure/save`, { name, departureTimeUtc, seed });
  }

  export(missionId: number, resultId: number, format: ExportFormat) {
    return this.http.get(`${this.url(missionId)}/${resultId}/export`, {
      params: new HttpParams().set('format', format),
      responseType: 'blob',
      observe: 'response'
    });
  }
}
