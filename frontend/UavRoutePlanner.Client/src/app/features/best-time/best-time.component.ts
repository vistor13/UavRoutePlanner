import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MissionApi } from '../../core/api/mission.api';
import { RouteApi } from '../../core/api/route.api';
import { errorMessage } from '../../core/http/error-message';
import { DepartureHour, DepartureRecommendation, Mission, MissionSummary, RouteResult } from '../../core/models';
import {
  floorToLocalHour, formatDuration, formatLocalDateTime, formatLocalHour, toLocalInput, toUtcHourIso, zoneLabel
} from '../../core/util/format';
import { MapComponent, MapMarker } from '../../shared/map/map.component';
import { AlertComponent } from '../../shared/ui/alert.component';
import { WindArrowComponent } from '../../shared/ui/wind-arrow.component';
import { RouteDetailsComponent } from '../missions/view/route-details.component';
import { RouteSummaryComponent } from '../missions/view/route-summary.component';

const MAX_HOURS = 72;
const HOUR_MS = 3_600_000;
const MAX_NAME = 200;

interface Preset {
  label: string;
  hours: number | 'tomorrow';
}

@Component({
  selector: 'app-best-time',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule, RouterLink, AlertComponent, MapComponent, WindArrowComponent,
    RouteSummaryComponent, RouteDetailsComponent
  ],
  templateUrl: './best-time.component.html',
  styleUrl: './best-time.component.css'
})
export class BestTimeComponent implements OnInit {
  private readonly missionApi = inject(MissionApi);
  private readonly routeApi = inject(RouteApi);
  private readonly router = inject(Router);

  readonly mission = input<string>();

  protected readonly zone = zoneLabel();
  protected readonly duration = formatDuration;
  protected readonly hour = formatLocalHour;
  protected readonly stamp = formatLocalDateTime;

  protected readonly presets: Preset[] = [
    { label: 'Next 6 h', hours: 6 },
    { label: 'Next 12 h', hours: 12 },
    { label: 'Next 24 h', hours: 24 },
    { label: 'Next 48 h', hours: 48 },
    { label: 'Tomorrow', hours: 'tomorrow' }
  ];

  protected readonly minLocal = toLocalInput(floorToLocalHour(new Date()));
  protected readonly maxLocal = toLocalInput(new Date(floorToLocalHour(new Date()).getTime() + (MAX_HOURS - 1) * HOUR_MS));

  protected readonly missions = signal<MissionSummary[]>([]);
  protected readonly loaded = signal(false);
  protected readonly missionId = signal<number | null>(null);
  protected readonly selectedMission = signal<Mission | null>(null);

  protected readonly from = signal('');
  protected readonly to = signal('');

  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<DepartureRecommendation | null>(null);
  protected readonly expanded = signal<string | null>(null);
  protected readonly picked = signal<string | null>(null);

  protected readonly saveOpen = signal(false);
  protected readonly saveName = signal('');
  protected readonly saving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected readonly hoursCount = computed(() => {
    const from = this.parse(this.from());
    const to = this.parse(this.to());
    return from === null || to === null ? null : Math.floor((to - from) / HOUR_MS) + 1;
  });

  protected readonly rangeError = computed(() => {
    const count = this.hoursCount();
    if (count === null) return 'Select both the start and the end of the range.';
    if (count < 1) return 'The end must not be earlier than the start.';
    if (count > MAX_HOURS) return `The range is ${count} hours. The maximum is ${MAX_HOURS} hours.`;
    return null;
  });

  protected readonly totalObjects = computed(() => this.selectedMission()?.objects.length ?? 0);

  protected readonly bestRow = computed(() => this.result()?.hours.find(item => item.isBest) ?? null);

  protected readonly pickedRow = computed(() => {
    const iso = this.picked();
    return this.result()?.hours.find(item => item.timeUtc === iso) ?? null;
  });

  protected readonly nameError = computed(() => {
    const length = this.saveName().trim().length;
    return length === 0 ? 'Enter a mission name.' : length > MAX_NAME ? `The name is limited to ${MAX_NAME} characters.` : null;
  });

  protected readonly markers = computed<MapMarker[]>(() => this.markersFor(this.expandedRoute()));
  protected readonly path = computed<[number, number][]>(() => this.pathFor(this.expandedRoute()));

  private readonly expandedRoute = computed<RouteResult | null>(() => {
    const iso = this.expanded();
    return this.result()?.hours.find(item => item.timeUtc === iso)?.route ?? null;
  });

  constructor() {
    this.applyPreset(this.presets[2]);
  }

  ngOnInit(): void {
    this.missionApi.getAll().subscribe({
      next: items => {
        this.missions.set(items);
        this.loaded.set(true);

        const wanted = Number(this.mission());
        if (items.some(item => item.id === wanted)) this.selectMission(wanted);
      },
      error: (e: unknown) => {
        this.loaded.set(true);
        this.error.set(errorMessage(e));
      }
    });
  }

  protected onMissionChange(value: string): void {
    this.selectMission(value ? Number(value) : null);
  }

  protected applyPreset(preset: Preset): void {
    const now = floorToLocalHour(new Date());

    if (preset.hours === 'tomorrow') {
      const start = new Date(now);
      start.setDate(start.getDate() + 1);
      start.setHours(0, 0, 0, 0);
      const end = new Date(start);
      end.setHours(23, 0, 0, 0);
      this.from.set(toLocalInput(start));
      this.to.set(toLocalInput(end));
    } else {
      this.from.set(toLocalInput(now));
      this.to.set(toLocalInput(new Date(now.getTime() + (preset.hours - 1) * HOUR_MS)));
    }

    this.clearResult();
  }

  protected changeRange(field: 'from' | 'to', value: string): void {
    (field === 'from' ? this.from : this.to).set(value);
    this.clearResult();
  }

  protected run(): void {
    const id = this.missionId();
    const from = toUtcHourIso(this.from());
    const to = toUtcHourIso(this.to());
    if (id === null || this.rangeError() || !from || !to) return;

    this.busy.set(true);
    this.error.set(null);
    this.clearResult();

    this.routeApi.findBestDeparture(id, from, to, null).subscribe({
      next: result => {
        this.busy.set(false);
        this.result.set(result);
        this.picked.set(result.bestDepartureTimeUtc);
        this.expanded.set(result.bestDepartureTimeUtc);
      },
      error: (e: unknown) => {
        this.busy.set(false);
        this.error.set(errorMessage(e));
      }
    });
  }

  protected feasible(item: DepartureHour): boolean {
    return item.errorCode === null && item.route !== null;
  }

  protected reason(item: DepartureHour): string {
    return item.errorCode === 'TimeBudgetTooSmall' ? 'Time limit exceeded' : 'Not feasible';
  }

  protected toggle(item: DepartureHour): void {
    if (!this.feasible(item)) return;
    this.expanded.update(current => (current === item.timeUtc ? null : item.timeUtc));
  }

  protected pick(item: DepartureHour, event: Event): void {
    event.stopPropagation();
    if (this.feasible(item)) this.picked.set(item.timeUtc);
  }

  protected openSave(): void {
    const row = this.pickedRow();
    const mission = this.selectedMission();
    if (!row || !mission) return;

    const suffix = ` · ${formatLocalDateTime(row.timeUtc)}`;
    this.saveName.set(mission.name.slice(0, MAX_NAME - suffix.length) + suffix);
    this.saveError.set(null);
    this.saveOpen.set(true);
  }

  protected closeSave(): void {
    if (!this.saving()) this.saveOpen.set(false);
  }

  protected save(): void {
    const id = this.missionId();
    const result = this.result();
    const row = this.pickedRow();
    if (id === null || !result || !row || this.nameError()) return;

    this.saving.set(true);
    this.saveError.set(null);

    this.routeApi.saveDeparturePlan(id, this.saveName().trim(), row.timeUtc, result.seed).subscribe({
      next: created => {
        this.saving.set(false);
        void this.router.navigate(['/missions', created.id]);
      },
      error: (e: unknown) => {
        this.saving.set(false);
        this.saveError.set(errorMessage(e));
      }
    });
  }

  private selectMission(id: number | null): void {
    this.missionId.set(id);
    this.selectedMission.set(null);
    this.clearResult();
    this.error.set(null);
    if (id === null) return;

    this.missionApi.getById(id).subscribe({
      next: mission => {
        if (this.missionId() === id) this.selectedMission.set(mission);
      },
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }

  private clearResult(): void {
    this.result.set(null);
    this.expanded.set(null);
    this.picked.set(null);
    this.saveOpen.set(false);
  }

  private markersFor(route: RouteResult | null): MapMarker[] {
    const mission = this.selectedMission();
    if (!mission) return [];

    const visited = new Set(route?.legs.map(leg => leg.objectNumber).filter(n => n !== null));
    const sameEnds = mission.start.latitude === mission.finish.latitude && mission.start.longitude === mission.finish.longitude;

    const markers: MapMarker[] = [
      { id: 'start', ...mission.start, label: sameEnds ? 'S/F' : 'S', kind: 'start' }
    ];
    if (!sameEnds) markers.push({ id: 'finish', ...mission.finish, label: 'F', kind: 'finish' });

    for (const object of mission.objects) {
      markers.push({
        id: `o${object.number}`,
        latitude: object.latitude,
        longitude: object.longitude,
        label: `${object.number}`,
        kind: route ? (visited.has(object.number) ? 'visited' : 'skipped') : 'object'
      });
    }

    return markers;
  }

  private pathFor(route: RouteResult | null): [number, number][] {
    const mission = this.selectedMission();
    if (!mission || !route) return [];

    return [
      [mission.start.latitude, mission.start.longitude],
      ...route.legs.map(leg => [leg.latitude, leg.longitude] as [number, number])
    ];
  }

  private parse(value: string): number | null {
    if (!value) return null;
    const time = new Date(value).getTime();
    return Number.isNaN(time) ? null : floorToLocalHour(new Date(time)).getTime();
  }
}
