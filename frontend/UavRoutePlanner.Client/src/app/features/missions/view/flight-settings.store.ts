import { Injectable, computed, signal } from '@angular/core';
import { RouteRequest } from '../../../core/models';
import { formatLocalDateTime, toUtcHourIso, zoneLabel } from '../../../core/util/format';

export type WindSource = 'forecast' | 'manual' | 'none';
@Injectable()
export class FlightSettingsStore {
  readonly source = signal<WindSource>('forecast');
  readonly speed = signal<number | null>(5);
  readonly direction = signal<number | null>(270);
  readonly departure = signal('');
  readonly seed = signal<number | null>(null);

  readonly speedError = computed(() => {
    const value = this.speed();
    return value === null || !Number.isFinite(value) || value < 0 ? 'Enter 0 or more' : null;
  });

  readonly directionError = computed(() => {
    const value = this.direction();
    return value === null || !Number.isFinite(value) || value < 0 || value > 360 ? 'Enter 0 to 360' : null;
  });

  readonly departureIso = computed(() => {
    return this.source() === 'forecast' ? toUtcHourIso(this.departure()) : null;
  });

  readonly blocker = computed(() => {
    const source = this.source();
    if (source === 'manual' && (this.speedError() || this.directionError()))
      return 'Enter a valid wind speed and direction.';
    return null;
  });

  readonly summary = computed(() => {
    const source = this.source();
    if (source === 'none') return 'Route is calculated without wind.';
    if (source === 'manual') return `Manual wind: ${this.speed()} m/s from ${this.direction()}°.`;

    const iso = this.departureIso();
    return iso
      ? `Forecast wind at ${formatLocalDateTime(iso)} (${zoneLabel(iso)}).`
      : 'Forecast wind for the current hour.';
  });

  request(): RouteRequest {
    const source = this.source();

    return {
      mode: source === 'none' ? 'NoWind' : 'WithWind',
      departureTimeUtc: this.departureIso(),
      manualWind: source === 'manual'
        ? { speedMs: Number(this.speed()), directionFromDeg: Number(this.direction()) }
        : null,
      seed: this.seed()
    };
  }
}
