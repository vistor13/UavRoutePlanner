import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouteResult } from '../../../core/models';
import { formatDistance, formatDuration, formatUtcHour } from '../../../core/util/format';
import { WindArrowComponent } from '../../../shared/ui/wind-arrow.component';
import { RouteExportComponent } from './route-export.component';

@Component({
  selector: 'app-route-details',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WindArrowComponent, RouteExportComponent],
  template: `
    @let r = route();
    <section class="card">
      <div class="row between">
        <h2>Flight task</h2>
        <app-route-export [route]="r" />
      </div>

      <dl class="facts">
        <div><dt>Objects visited</dt><dd>{{ r.visitedObjectsCount }} of {{ r.totalObjectsCount }}</dd></div>
        <div><dt>Flight time</dt><dd>{{ duration(r.totalTimeS) }} (limit {{ duration(r.timeLimitS) }})</dd></div>
        <div><dt>Departure</dt><dd>{{ hour(r.departureTimeUtc) }}</dd></div>
        <div>
          <dt>Wind</dt>
          <dd>
            @if (r.windMode === 'NoWind') {
              none
            } @else {
              <app-wind-arrow [speedMs]="r.windSpeedMs" [directionFromDeg]="r.windDirectionFromDeg" />
            }
          </dd>
        </div>
      </dl>

      <table>
        <thead>
          <tr>
            <th>#</th><th>Point</th><th>Distance</th><th>Track, &deg;</th>
            <th>Heading, &deg;</th><th>Ground speed, m/s</th><th>Time</th>
          </tr>
        </thead>
        <tbody>
          @for (leg of r.legs; track leg.order) {
            <tr>
              <td>{{ leg.order }}</td>
              <td>{{ leg.pointName }}</td>
              <td>{{ distance(leg.distanceM) }}</td>
              <td>{{ leg.trackDeg.toFixed(1) }}</td>
              <td>{{ leg.headingDeg.toFixed(1) }}</td>
              <td>{{ leg.groundSpeedMs.toFixed(1) }}</td>
              <td>{{ duration(leg.timeS) }}</td>
            </tr>
          }
        </tbody>
      </table>

      @if (r.skippedObjects.length > 0) {
        <h3>Skipped objects</h3>
        <ul class="chips">
          @for (object of r.skippedObjects; track object.number) {
            <li>{{ object.number }}. {{ object.name }}</li>
          }
        </ul>
      }
    </section>
  `
})
export class RouteDetailsComponent {
  readonly route = input.required<RouteResult>();

  protected readonly duration = formatDuration;
  protected readonly distance = formatDistance;
  protected readonly hour = formatUtcHour;
}
