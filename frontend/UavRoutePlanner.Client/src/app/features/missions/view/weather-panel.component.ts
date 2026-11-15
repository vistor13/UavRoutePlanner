import { ChangeDetectionStrategy, Component, OnInit, inject, input, signal } from '@angular/core';
import { WeatherApi } from '../../../core/api/weather.api';
import { errorMessage } from '../../../core/http/error-message';
import { WeatherForecast } from '../../../core/models';
import { formatUtcHour } from '../../../core/util/format';
import { AlertComponent } from '../../../shared/ui/alert.component';
import { WindArrowComponent } from '../../../shared/ui/wind-arrow.component';

const VISIBLE_HOURS = 12;

@Component({
  selector: 'app-weather-panel',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AlertComponent, WindArrowComponent],
  template: `
    <section class="card">
      <h2>Wind forecast</h2>
      <app-alert [message]="error()" />
      @if (forecast(); as f) {
        <p class="muted">At {{ f.altitudeM }} m, near {{ f.latitude }}, {{ f.longitude }}</p>
        <table>
          <tbody>
            @for (hour of f.hours.slice(0, visible); track hour.timeUtc) {
              <tr>
                <td>{{ format(hour.timeUtc) }}</td>
                <td><app-wind-arrow [speedMs]="hour.speedMs" [directionFromDeg]="hour.directionFromDeg" /></td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
  `
})
export class WeatherPanelComponent implements OnInit {
  private readonly api = inject(WeatherApi);

  readonly missionId = input.required<number>();

  protected readonly forecast = signal<WeatherForecast | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly visible = VISIBLE_HOURS;
  protected readonly format = formatUtcHour;

  ngOnInit(): void {
    this.api.getForMission(this.missionId()).subscribe({
      next: forecast => this.forecast.set(forecast),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }
}
