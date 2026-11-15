import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { DepartureRecommendation } from '../../../core/models';
import { formatDuration, formatUtcHour } from '../../../core/util/format';
import { WindArrowComponent } from '../../../shared/ui/wind-arrow.component';

@Component({
  selector: 'app-departure-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WindArrowComponent],
  template: `
    @let rec = recommendation();
    <section class="card">
      <h2>Departure time search</h2>
      <p>Best departure: <strong>{{ hour(rec.bestDepartureTimeUtc) }}</strong>. Its route is shown on the map and saved in history.</p>
      @if (rec.noWindBaseline) {
        <p class="muted">
          No-wind baseline: {{ rec.noWindBaseline.visitedObjectsCount }} objects, {{ duration(rec.noWindBaseline.totalTimeS) }}.
        </p>
      }

      <table>
        <thead>
          <tr><th>Hour</th><th>Wind</th><th>Objects</th><th>Time</th><th></th></tr>
        </thead>
        <tbody>
          @for (item of rec.hours; track item.timeUtc) {
            <tr [class.best]="item.isBest">
              <td>{{ hour(item.timeUtc) }}</td>
              <td><app-wind-arrow [speedMs]="item.windSpeedMs" [directionFromDeg]="item.windDirectionFromDeg" /></td>
              <td>{{ item.visitedObjectsCount ?? '—' }}</td>
              <td>{{ item.totalTimeS === null ? '—' : duration(item.totalTimeS) }}</td>
              <td>{{ item.isBest ? 'best' : (item.errorCode ?? '') }}</td>
            </tr>
          }
        </tbody>
      </table>
    </section>
  `
})
export class DepartureTableComponent {
  readonly recommendation = input.required<DepartureRecommendation>();

  protected readonly duration = formatDuration;
  protected readonly hour = formatUtcHour;
}
