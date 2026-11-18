import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { MissionApi } from '../../../core/api/mission.api';
import { RouteApi } from '../../../core/api/route.api';
import { WeatherApi } from '../../../core/api/weather.api';
import { errorMessage } from '../../../core/http/error-message';
import { formatLocalDateTime, toLocalInput, zoneLabel } from '../../../core/util/format';
import { Mission, RouteComparison, RouteRequest, RouteResult, WeatherForecast } from '../../../core/models';
import { MapComponent, MapMarker } from '../../../shared/map/map.component';
import { AlertComponent } from '../../../shared/ui/alert.component';
import { StepsComponent } from '../../../shared/ui/steps/steps.component';
import { ComparisonPanelComponent } from './comparison-panel.component';
import { FlightSettingsStore } from './flight-settings.store';
import { RouteControlsComponent } from './route-controls.component';
import { RouteDetailsComponent } from './route-details.component';
import { RouteHistoryComponent } from './route-history.component';
import { RouteSummaryComponent } from './route-summary.component';

@Component({
  selector: 'app-mission-view',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [FlightSettingsStore],
  imports: [
    RouterLink, MapComponent, AlertComponent, RouteControlsComponent, RouteDetailsComponent,
    ComparisonPanelComponent, RouteSummaryComponent, RouteHistoryComponent, StepsComponent
  ],
  templateUrl: './mission-view.component.html',
  styleUrl: './mission-view.component.css'
})
export class MissionViewComponent implements OnInit {
  private readonly missionApi = inject(MissionApi);
  private readonly routeApi = inject(RouteApi);
  private readonly weatherApi = inject(WeatherApi);
  private readonly store = inject(FlightSettingsStore);

  readonly id = input.required<string>();

  protected readonly mission = signal<Mission | null>(null);
  protected readonly routes = signal<RouteResult[]>([]);
  protected readonly selected = signal<RouteResult | null>(null);
  protected readonly comparison = signal<RouteComparison | null>(null);
  protected readonly forecast = signal<WeatherForecast | null>(null);
  protected readonly forecastError = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly stamp = formatLocalDateTime;
  protected readonly zone = zoneLabel;

  protected readonly markers = computed<MapMarker[]>(() => {
    const mission = this.mission();
    if (!mission) return [];

    const route = this.selected();
    const visited = new Set(route?.legs.map(leg => leg.objectNumber).filter(n => n !== null));
    const sameEnds = samePoint(mission.start, mission.finish);

    const result: MapMarker[] = [
      { id: 'start', ...mission.start, label: sameEnds ? 'S/F' : 'S', kind: 'start' }
    ];
    if (!sameEnds) result.push({ id: 'finish', ...mission.finish, label: 'F', kind: 'finish' });

    for (const object of mission.objects) {
      result.push({
        id: `o${object.number}`,
        latitude: object.latitude,
        longitude: object.longitude,
        label: `${object.number}`,
        kind: route ? (visited.has(object.number) ? 'visited' : 'skipped') : 'object'
      });
    }

    return result;
  });

  protected readonly path = computed<[number, number][]>(() => {
    const mission = this.mission();
    const route = this.selected();
    if (!mission || !route) return [];

    return [
      [mission.start.latitude, mission.start.longitude],
      ...route.legs.map(leg => [leg.latitude, leg.longitude] as [number, number])
    ];
  });

  ngOnInit(): void {
    const missionId = Number(this.id());

    this.missionApi.getById(missionId).subscribe({
      next: mission => {
        this.mission.set(mission);
        if (mission.plannedDepartureUtc) this.store.departure.set(toLocalInput(new Date(mission.plannedDepartureUtc)));
      },
      error: (e: unknown) => this.error.set(errorMessage(e))
    });

    this.weatherApi.getForMission(missionId).subscribe({
      next: forecast => this.forecast.set(forecast),
      error: (e: unknown) => this.forecastError.set(errorMessage(e))
    });

    this.loadRoutes(missionId, true);
  }

  protected select(route: RouteResult): void {
    this.selected.set(route);
  }

  protected build(request: RouteRequest): void {
    this.run(this.routeApi.build(this.missionId, request), route => {
      this.comparison.set(null);
      this.selected.set(route);
    });
  }

  protected compare(request: RouteRequest): void {
    this.run(this.routeApi.compare(this.missionId, request), comparison => {
      this.comparison.set(comparison);
      this.selected.set(comparison.withWind);
    });
  }

  private get missionId(): number {
    return Number(this.id());
  }

  private run<T>(source: Observable<T>, onSuccess: (value: T) => void): void {
    this.busy.set(true);
    this.error.set(null);

    source.subscribe({
      next: value => {
        onSuccess(value);
        this.busy.set(false);
        this.loadRoutes(this.missionId, false);
      },
      error: (e: unknown) => {
        this.busy.set(false);
        this.error.set(errorMessage(e));
      }
    });
  }

  private loadRoutes(missionId: number, selectLatest: boolean): void {
    this.routeApi.getAll(missionId).subscribe({
      next: routes => {
        this.routes.set(routes);
        if (selectLatest && routes.length > 0) this.selected.set(routes[routes.length - 1]);
      },
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }
}

function samePoint(a: { latitude: number; longitude: number }, b: { latitude: number; longitude: number }): boolean {
  return a.latitude === b.latitude && a.longitude === b.longitude;
}
