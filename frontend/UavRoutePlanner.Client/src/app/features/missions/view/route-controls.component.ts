import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouteRequest } from '../../../core/models';
import { toUtcHourIso } from '../../../core/util/format';

type WindSource = 'forecast' | 'manual' | 'none';

export interface BestDepartureRequest {
  hours: number | null;
  seed: number | null;
}

@Component({
  selector: 'app-route-controls',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
  template: `
    <section class="card">
      <h2>Build route</h2>

      <label>Wind
        <select [ngModel]="source()" (ngModelChange)="source.set($event)">
          <option value="forecast">Forecast (Open-Meteo)</option>
          <option value="manual">Manual</option>
          <option value="none">No wind</option>
        </select>
      </label>

      @if (source() === 'manual') {
        <div class="grid two">
          <label>Speed, m/s <input type="number" step="any" [ngModel]="speed()" (ngModelChange)="speed.set($event)"></label>
          <label>Direction from, &deg; <input type="number" step="any" [ngModel]="direction()" (ngModelChange)="direction.set($event)"></label>
        </div>
      }

      @if (source() === 'forecast') {
        <label>Departure time (optional, default: now)
          <input type="datetime-local" [ngModel]="departure()" (ngModelChange)="departure.set($event)">
        </label>
      }

      <label>Seed (optional, for repeatable results)
        <input type="number" step="1" [ngModel]="seed()" (ngModelChange)="seed.set($event)">
      </label>

      <div class="row wrap">
        <button class="primary" type="button" [disabled]="busy()" (click)="build()">Build route</button>
        <button type="button" [disabled]="busy()" (click)="compare()">Compare with / without wind</button>
      </div>

      <h2>Best departure time</h2>
      <label>Hours to review (1..72)
        <input type="number" step="1" [ngModel]="hours()" (ngModelChange)="hours.set($event)">
      </label>
      <button type="button" [disabled]="busy()" (click)="best()">Find best departure</button>
    </section>
  `
})
export class RouteControlsComponent {
  readonly busy = input(false);

  readonly buildRequested = output<RouteRequest>();
  readonly compareRequested = output<RouteRequest>();
  readonly bestRequested = output<BestDepartureRequest>();

  protected readonly source = signal<WindSource>('forecast');
  protected readonly speed = signal<number>(5);
  protected readonly direction = signal<number>(270);
  protected readonly departure = signal('');
  protected readonly seed = signal<number | null>(null);
  protected readonly hours = signal<number>(24);

  protected build(): void {
    this.buildRequested.emit(this.request());
  }

  protected compare(): void {
    const request = this.request();
    this.compareRequested.emit({ ...request, mode: 'WithWind' });
  }

  protected best(): void {
    this.bestRequested.emit({ hours: this.hours() || null, seed: this.seed() });
  }

  private request(): RouteRequest {
    const source = this.source();

    return {
      mode: source === 'none' ? 'NoWind' : 'WithWind',
      departureTimeUtc: source === 'forecast' ? toUtcHourIso(this.departure()) : null,
      manualWind: source === 'manual'
        ? { speedMs: Number(this.speed()), directionFromDeg: Number(this.direction()) }
        : null,
      seed: this.seed()
    };
  }
}
