import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MissionApi } from '../../../core/api/mission.api';
import { RouteApi } from '../../../core/api/route.api';
import { UavProfileApi } from '../../../core/api/uav-profile.api';
import { errorMessage } from '../../../core/http/error-message';
import {
  AlgorithmSettings, DEFAULT_ALGORITHM, GeoPoint, ImportedObject, Mission, MissionInput, SurveyObjectInput, UavProfile
} from '../../../core/models';
import { MapClick, MapComponent, MapMarker } from '../../../shared/map/map.component';
import { AlertComponent } from '../../../shared/ui/alert.component';
import { StepsComponent } from '../../../shared/ui/steps/steps.component';
import { AlgorithmSettingsComponent, isAlgorithmValid } from './algorithm-settings.component';
import { ImportObjectsComponent, ImportedFile, ImportResult } from './import-objects.component';

type PlacingMode = 'start' | 'finish' | 'object';

interface EditorObject extends SurveyObjectInput {
  source?: number;
}

interface ImportSource {
  id: number;
  name: string;
}

const MAX_OBJECTS = 100;

@Component({
  selector: 'app-mission-editor',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule, RouterLink, MapComponent, AlertComponent, AlgorithmSettingsComponent, ImportObjectsComponent, StepsComponent
  ],
  templateUrl: './mission-editor.component.html',
  styleUrl: './mission-editor.component.css'
})
export class MissionEditorComponent implements OnInit {
  private readonly missionApi = inject(MissionApi);
  private readonly profileApi = inject(UavProfileApi);
  private readonly routeApi = inject(RouteApi);
  private readonly router = inject(Router);

  readonly id = input<string>();

  protected readonly editing = computed(() => this.id() !== undefined);
  protected readonly loading = signal(false);
  protected readonly routesCount = signal(0);
  protected readonly fitMap = signal(false);
  protected readonly cancelLink = computed(() => (this.editing() ? ['/missions', this.id()] : ['/missions']));

  protected readonly name = signal('');
  protected readonly profiles = signal<UavProfile[]>([]);
  protected readonly profileId = signal<number | null>(null);
  protected readonly mode = signal<PlacingMode>('start');
  protected readonly start = signal<GeoPoint | null>(null);
  protected readonly finish = signal<GeoPoint | null>(null);
  protected readonly finishSameAsStart = signal(true);
  protected readonly objects = signal<EditorObject[]>([]);
  protected readonly sources = signal<ImportSource[]>([]);
  protected readonly algorithm = signal<AlgorithmSettings>({ ...DEFAULT_ALGORITHM });
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

  protected readonly markers = computed<MapMarker[]>(() => {
    const result: MapMarker[] = [];
    const start = this.start();
    const finish = this.finishSameAsStart() ? null : this.finish();

    if (start) {
      result.push({ id: 'start', ...start, label: this.finishSameAsStart() ? 'S/F' : 'S', kind: 'start' });
    }
    if (finish) result.push({ id: 'finish', ...finish, label: 'F', kind: 'finish' });

    this.objects().forEach((object, index) =>
      result.push({ id: `o${index}`, latitude: object.latitude, longitude: object.longitude, label: `${index + 1}`, kind: 'object' }));

    return result;
  });

  protected readonly importedFiles = computed<ImportedFile[]>(() =>
    this.sources()
      .map(source => ({ ...source, count: this.objects().filter(object => object.source === source.id).length }))
      .filter(file => file.count > 0));

  protected readonly missing = computed(() => {
    const result: string[] = [];
    if (this.name().trim().length === 0) result.push('a mission name');
    if (this.profileId() === null) result.push('a UAV profile');
    if (this.start() === null) result.push('a start point');
    if (!this.finishSameAsStart() && this.finish() === null) result.push('a finish point');
    if (this.objects().length === 0) result.push('at least one object');
    if (!isAlgorithmValid(this.algorithm())) result.push('valid algorithm settings');
    return result;
  });

  protected readonly hint = computed(() => {
    const missing = this.missing();
    return this.loading() || missing.length === 0 ? null : `Add ${missing.join(', ')}.`;
  });

  protected readonly canSave = computed(() => this.missing().length === 0);

  private nextSourceId = 1;

  ngOnInit(): void {
    const id = this.id();

    if (id === undefined) {
      this.profileApi.getAll().subscribe({
        next: profiles => {
          this.profiles.set(profiles);
          if (profiles.length > 0) this.profileId.set(profiles[0].id);
        },
        error: (e: unknown) => this.error.set(errorMessage(e))
      });
      return;
    }

    const missionId = Number(id);
    this.loading.set(true);

    forkJoin({
      profiles: this.profileApi.getAll(),
      mission: this.missionApi.getById(missionId),
      routes: this.routeApi.getAll(missionId)
    }).subscribe({
      next: ({ profiles, mission, routes }) => {
        this.profiles.set(profiles);
        this.routesCount.set(routes.length);
        this.load(mission, profiles);
        this.loading.set(false);
      },
      error: (e: unknown) => {
        this.loading.set(false);
        this.error.set(errorMessage(e));
      }
    });
  }

  protected setSameFinish(value: boolean): void {
    this.finishSameAsStart.set(value);
    if (value && this.mode() === 'finish') this.mode.set('object');
  }

  protected onMapClick(click: MapClick): void {
    const point: GeoPoint = { latitude: round(click.latitude), longitude: round(click.longitude) };

    switch (this.mode()) {
      case 'start':
        this.start.set(point);
        this.mode.set(this.finishSameAsStart() ? 'object' : 'finish');
        break;
      case 'finish':
        this.finish.set(point);
        this.mode.set('object');
        break;
      case 'object':
        this.addObjects([{ name: `Object ${this.objects().length + 1}`, ...point }]);
        break;
    }
  }

  protected addImported(result: ImportResult): void {
    const id = this.nextSourceId++;
    this.sources.update(list => [...list, { id, name: result.name }]);
    this.addObjects(result.objects.map((object: ImportedObject) => ({ ...object, source: id })));
  }

  protected removeImport(id: number): void {
    this.objects.update(list => list.filter(object => object.source !== id));
    this.sources.update(list => list.filter(source => source.id !== id));
  }

  protected rename(index: number, name: string): void {
    this.objects.update(list => list.map((item, i) => (i === index ? { ...item, name } : item)));
  }

  protected removeObject(index: number): void {
    this.objects.update(list => list.filter((_, i) => i !== index));
  }

  protected save(): void {
    const start = this.start();
    const profileId = this.profileId();
    if (!start || profileId === null) return;

    const input: MissionInput = {
      name: this.name().trim(),
      start,
      finish: this.finishSameAsStart() ? start : this.finish()!,
      uavProfileId: profileId,
      objects: this.objects().map(({ name, latitude, longitude }) => ({ name, latitude, longitude })),
      algorithm: this.algorithm()
    };

    this.busy.set(true);
    this.error.set(null);

    const id = this.id();
    const request = id === undefined ? this.missionApi.create(input) : this.missionApi.update(Number(id), input);

    request.subscribe({
      next: mission => void this.router.navigate(['/missions', mission.id]),
      error: (e: unknown) => {
        this.busy.set(false);
        this.error.set(errorMessage(e));
      }
    });
  }

  private load(mission: Mission, profiles: UavProfile[]): void {
    const sameEnds = mission.start.latitude === mission.finish.latitude
      && mission.start.longitude === mission.finish.longitude;

    this.name.set(mission.name);
    this.profileId.set(profiles.some(p => p.id === mission.uavProfileId) ? mission.uavProfileId : null);
    this.start.set({ latitude: mission.start.latitude, longitude: mission.start.longitude });
    this.finish.set(sameEnds ? null : { latitude: mission.finish.latitude, longitude: mission.finish.longitude });
    this.finishSameAsStart.set(sameEnds);
    this.objects.set(mission.objects.map(o => ({ name: o.name, latitude: o.latitude, longitude: o.longitude })));
    this.algorithm.set({ ...mission.algorithm });
    this.mode.set('object');

    this.fitMap.set(true);
    setTimeout(() => this.fitMap.set(false));
  }

  private addObjects(items: EditorObject[]): void {
    this.objects.update(list => [...list, ...items].slice(0, MAX_OBJECTS));
  }
}

function round(value: number): number {
  return Math.round(value * 1_000_000) / 1_000_000;
}
