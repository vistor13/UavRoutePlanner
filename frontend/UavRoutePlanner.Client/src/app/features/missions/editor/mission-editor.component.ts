import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MissionApi } from '../../../core/api/mission.api';
import { UavProfileApi } from '../../../core/api/uav-profile.api';
import { errorMessage } from '../../../core/http/error-message';
import {
  AlgorithmSettings, DEFAULT_ALGORITHM, GeoPoint, ImportedObject, SurveyObjectInput, UavProfile
} from '../../../core/models';
import { MapClick, MapComponent, MapMarker } from '../../../shared/map/map.component';
import { AlertComponent } from '../../../shared/ui/alert.component';
import { AlgorithmSettingsComponent } from './algorithm-settings.component';
import { ImportObjectsComponent } from './import-objects.component';

type PlacingMode = 'start' | 'finish' | 'object';

const MAX_OBJECTS = 100;

@Component({
  selector: 'app-mission-editor',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule, RouterLink, MapComponent, AlertComponent, AlgorithmSettingsComponent, ImportObjectsComponent
  ],
  template: `
    <h1>New mission</h1>
    <app-alert [message]="error()" />

    <div class="editor">
      <section class="card side">
        <label>Mission name <input name="name" [ngModel]="name()" (ngModelChange)="name.set($event)"></label>

        <label>UAV profile
          <select [ngModel]="profileId()" (ngModelChange)="profileId.set($event)">
            <option [ngValue]="null" disabled>Select a profile</option>
            @for (profile of profiles(); track profile.id) {
              <option [ngValue]="profile.id">{{ profile.name }}</option>
            }
          </select>
        </label>
        @if (profiles().length === 0) {
          <p class="muted">No profiles yet. <a routerLink="/profiles">Create one</a> first.</p>
        }

        <fieldset>
          <legend>Click on the map to place</legend>
          <label class="inline"><input type="radio" name="mode" value="start" [ngModel]="mode()" (ngModelChange)="mode.set($event)"> Start</label>
          <label class="inline"><input type="radio" name="mode" value="finish" [ngModel]="mode()" (ngModelChange)="mode.set($event)"> Finish</label>
          <label class="inline"><input type="radio" name="mode" value="object" [ngModel]="mode()" (ngModelChange)="mode.set($event)"> Objects</label>
          <label class="inline">
            <input type="checkbox" [ngModel]="finishSameAsStart()" (ngModelChange)="setSameFinish($event)">
            Finish is the same as start
          </label>
        </fieldset>

        <app-import-objects (imported)="addImported($event)" />

        <h2>Objects ({{ objects().length }})</h2>
        <ul class="list compact">
          @for (object of objects(); track $index) {
            <li>
              <span class="num">{{ $index + 1 }}</span>
              <input [ngModel]="object.name" (ngModelChange)="rename($index, $event)">
              <button type="button" class="danger" (click)="removeObject($index)">&times;</button>
            </li>
          }
        </ul>

        <details>
          <summary>Algorithm settings</summary>
          <app-algorithm-settings [(settings)]="algorithm" />
        </details>

        <div class="row">
          <button class="primary" type="button" [disabled]="!canSave() || busy()" (click)="save()">Create mission</button>
          <a class="button" routerLink="/missions">Cancel</a>
        </div>
      </section>

      <section class="mapbox">
        <app-map [markers]="markers()" (mapClicked)="onMapClick($event)" />
      </section>
    </div>
  `
})
export class MissionEditorComponent implements OnInit {
  private readonly missionApi = inject(MissionApi);
  private readonly profileApi = inject(UavProfileApi);
  private readonly router = inject(Router);

  protected readonly name = signal('');
  protected readonly profiles = signal<UavProfile[]>([]);
  protected readonly profileId = signal<number | null>(null);
  protected readonly mode = signal<PlacingMode>('start');
  protected readonly start = signal<GeoPoint | null>(null);
  protected readonly finish = signal<GeoPoint | null>(null);
  protected readonly finishSameAsStart = signal(true);
  protected readonly objects = signal<SurveyObjectInput[]>([]);
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

  protected readonly canSave = computed(() =>
    this.name().trim().length > 0
    && this.profileId() !== null
    && this.start() !== null
    && (this.finishSameAsStart() || this.finish() !== null)
    && this.objects().length > 0);

  ngOnInit(): void {
    this.profileApi.getAll().subscribe({
      next: profiles => {
        this.profiles.set(profiles);
        if (profiles.length > 0) this.profileId.set(profiles[0].id);
      },
      error: (e: unknown) => this.error.set(errorMessage(e))
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

  protected addImported(imported: ImportedObject[]): void {
    this.addObjects(imported);
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

    this.busy.set(true);
    this.error.set(null);

    this.missionApi
      .create({
        name: this.name().trim(),
        start,
        finish: this.finishSameAsStart() ? start : this.finish()!,
        uavProfileId: profileId,
        objects: this.objects(),
        algorithm: this.algorithm()
      })
      .subscribe({
        next: mission => void this.router.navigate(['/missions', mission.id]),
        error: (e: unknown) => {
          this.busy.set(false);
          this.error.set(errorMessage(e));
        }
      });
  }

  private addObjects(items: SurveyObjectInput[]): void {
    this.objects.update(list => [...list, ...items].slice(0, MAX_OBJECTS));
  }
}

function round(value: number): number {
  return Math.round(value * 1_000_000) / 1_000_000;
}
