import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RouteRequest, WeatherForecast } from '../../../core/models';
import { toLocalInput, floorToLocalHour, zoneLabel } from '../../../core/util/format';
import { HintComponent } from '../../../shared/ui/hint/hint.component';
import { SegmentedComponent, SegmentOption } from '../../../shared/ui/segmented/segmented.component';
import { FlightSettingsStore, WindSource } from './flight-settings.store';
import { WeatherPanelComponent } from './weather-panel.component';

const FORECAST_SPAN_HOURS = 71;

@Component({
  selector: 'app-route-controls',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, RouterLink, HintComponent, SegmentedComponent, WeatherPanelComponent],
  templateUrl: './route-controls.component.html',
  styleUrl: './route-controls.component.css'
})
export class RouteControlsComponent {
  protected readonly store = inject(FlightSettingsStore);

  readonly missionId = input.required<number>();
  readonly busy = input(false);
  readonly forecast = input<WeatherForecast | null>(null);
  readonly forecastError = input<string | null>(null);

  readonly buildRequested = output<RouteRequest>();
  readonly compareRequested = output<RouteRequest>();

  protected readonly advancedOpen = signal(false);
  protected readonly zone = zoneLabel();

  protected readonly windOptions: SegmentOption<WindSource>[] = [
    { value: 'forecast', label: 'Forecast' },
    { value: 'manual', label: 'Manual' },
    { value: 'none', label: 'No wind' }
  ];

  protected readonly minLocal = toLocalInput(floorToLocalHour(new Date()));
  protected readonly maxLocal = toLocalInput(new Date(floorToLocalHour(new Date()).getTime() + FORECAST_SPAN_HOURS * 3_600_000));

  protected toggleAdvanced(): void {
    this.advancedOpen.update(value => !value);
  }

  protected build(): void {
    this.buildRequested.emit(this.store.request());
  }

  protected compare(): void {
    this.compareRequested.emit({ ...this.store.request(), mode: 'WithWind' });
  }
}
