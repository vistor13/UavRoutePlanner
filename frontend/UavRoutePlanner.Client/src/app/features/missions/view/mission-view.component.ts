import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { MissionApi } from '../../../core/api/mission.api';
import { RouteApi } from '../../../core/api/route.api';
import { errorMessage } from '../../../core/http/error-message';
import {
  DepartureRecommendation, Mission, RouteComparison, RouteRequest, RouteResult
} from '../../../core/models';
import { MapComponent, MapMarker } from '../../../shared/map/map.component';
import { AlertComponent } from '../../../shared/ui/alert.component';
import { ComparisonPanelComponent } from './comparison-panel.component';
import { DepartureTableComponent } from './departure-table.component';
import { BestDepartureRequest, RouteControlsComponent } from './route-controls.component';
import { RouteDetailsComponent } from './route-details.component';
import { WeatherPanelComponent } from './weather-panel.component';

@Component({
  selector: 'app-mission-view',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink, DatePipe, MapComponent, AlertComponent, RouteControlsComponent, RouteDetailsComponent,
    ComparisonPanelComponent, DepartureTableComponent, WeatherPanelComponent
  ],
  template: `
    @if (mission(); as m) {
      <div class="row between">
        <h1>{{ m.name }}</h1>
        <a class="button" routerLink="/missions">Back to missions</a>
      </div>
      <p class="muted">
        UAV: {{ m.uav.airspeedMs }} m/s, {{ m.uav.maxFlightTimeMin }} min, reserve {{ m.uav.reservePercent }}%,
        altitude {{ m.uav.flightAltitudeM }} m &middot; {{ m.objects.length }} objects
      </p>

      <app-alert [message]="error()" />

      <div class="view">
        <section class="mapbox">
          <app-map [markers]="markers()" [path]="path()" [autoFit]="true" />
        </section>

        <aside class="side">
          <app-route-controls
            [busy]="busy()"
            (buildRequested)="build($event)"
            (compareRequested)="compare($event)"
            (bestRequested)="findBest($event)" />

          <section class="card">
            <h2>History ({{ routes().length }})</h2>
            @if (routes().length === 0) {
              <p class="muted">No routes built yet.</p>
            }
            <ul class="list compact">
              @for (route of routes(); track route.id) {
                <li [class.selected]="route.id === selected()?.id">
                  <button class="link" type="button" (click)="select(route)">
                    {{ route.createdAt | date: 'dd.MM HH:mm' }} &middot;
                    {{ route.windMode === 'NoWind' ? 'no wind' : 'wind' }} &middot;
                    {{ route.visitedObjectsCount }}/{{ route.totalObjectsCount }}
                  </button>
                </li>
              }
            </ul>
          </section>

          <app-weather-panel [missionId]="m.id" />
        </aside>
      </div>

      <div class="stack">
        @if (comparison(); as c) {
          <app-comparison-panel [comparison]="c" (selected)="select($event)" />
        }
        @if (recommendation(); as r) {
          <app-departure-table [recommendation]="r" />
        }
        @if (selected(); as route) {
          <app-route-details [route]="route" />
        }
      </div>
    } @else {
      <app-alert [message]="error()" />
    }
  `
})
export class MissionViewComponent implements OnInit {
  private readonly missionApi = inject(MissionApi);
  private readonly routeApi = inject(RouteApi);

  readonly id = input.required<string>();

  protected readonly mission = signal<Mission | null>(null);
  protected readonly routes = signal<RouteResult[]>([]);
  protected readonly selected = signal<RouteResult | null>(null);
  protected readonly comparison = signal<RouteComparison | null>(null);
  protected readonly recommendation = signal<DepartureRecommendation | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

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
      next: mission => this.mission.set(mission),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });

    this.loadRoutes(missionId, true);
  }

  protected select(route: RouteResult): void {
    this.selected.set(route);
  }

  protected build(request: RouteRequest): void {
    this.run(this.routeApi.build(this.missionId, request), route => {
      this.comparison.set(null);
      this.recommendation.set(null);
      this.selected.set(route);
    });
  }

  protected compare(request: RouteRequest): void {
    this.run(this.routeApi.compare(this.missionId, request), comparison => {
      this.recommendation.set(null);
      this.comparison.set(comparison);
      this.selected.set(comparison.withWind);
    });
  }

  protected findBest(request: BestDepartureRequest): void {
    this.run(this.routeApi.findBestDeparture(this.missionId, request.hours, request.seed), recommendation => {
      this.comparison.set(null);
      this.recommendation.set(recommendation);
      this.selected.set(recommendation.bestRoute);
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
