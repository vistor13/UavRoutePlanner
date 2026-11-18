import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { WeatherForecast } from '../../../core/models';
import { formatLocalHour, zoneLabel } from '../../../core/util/format';
import { AlertComponent } from '../../../shared/ui/alert.component';
import { WindArrowComponent } from '../../../shared/ui/wind-arrow.component';

const PREVIEW_HOURS = 6;

@Component({
  selector: 'app-weather-panel',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AlertComponent, WindArrowComponent],
  templateUrl: './weather-panel.component.html',
  styleUrl: './weather-panel.component.css'
})
export class WeatherPanelComponent {
  readonly forecast = input<WeatherForecast | null>(null);
  readonly error = input<string | null>(null);

  protected readonly expanded = signal(false);
  protected readonly zone = zoneLabel();
  protected readonly format = formatLocalHour;

  protected readonly hours = computed(() => {
    const forecast = this.forecast();
    if (!forecast) return [];
    return this.expanded() ? forecast.hours : forecast.hours.slice(0, PREVIEW_HOURS);
  });

  protected readonly canExpand = computed(() => (this.forecast()?.hours.length ?? 0) > PREVIEW_HOURS);

  protected toggle(): void {
    this.expanded.update(value => !value);
  }
}
