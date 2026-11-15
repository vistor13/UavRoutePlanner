import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from '../api-config';
import { WeatherForecast } from '../models';

@Injectable({ providedIn: 'root' })
export class WeatherApi {
  private readonly http = inject(HttpClient);

  getForMission(missionId: number) {
    return this.http.get<WeatherForecast>(`${API_URL}/missions/${missionId}/weather`);
  }
}
