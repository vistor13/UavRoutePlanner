import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouteComparison, RouteResult } from '../../../core/models';
import { formatDuration } from '../../../core/util/format';

@Component({
  selector: 'app-comparison-panel',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let c = comparison();
    <section class="card">
      <h2>Wind impact</h2>
      <div class="grid two">
        <div>
          <h3>With wind</h3>
          <p>{{ c.withWind.visitedObjectsCount }} of {{ c.withWind.totalObjectsCount }} objects</p>
          <p>{{ duration(c.withWind.totalTimeS) }}</p>
          <button type="button" (click)="selected.emit(c.withWind)">Show on map</button>
        </div>
        <div>
          <h3>No wind</h3>
          <p>{{ c.noWind.visitedObjectsCount }} of {{ c.noWind.totalObjectsCount }} objects</p>
          <p>{{ duration(c.noWind.totalTimeS) }}</p>
          <button type="button" (click)="selected.emit(c.noWind)">Show on map</button>
        </div>
      </div>
      <p class="summary">
        Objects lost because of wind: <strong>{{ c.objectsLostDueToWind }}</strong>.
        Time difference: <strong>{{ sign(c.timeDifferenceS) }}{{ duration(abs(c.timeDifferenceS)) }}</strong>.
      </p>
    </section>
  `
})
export class ComparisonPanelComponent {
  readonly comparison = input.required<RouteComparison>();
  readonly selected = output<RouteResult>();

  protected readonly duration = formatDuration;
  protected readonly abs = Math.abs;

  protected sign(seconds: number): string {
    return seconds > 0 ? '+' : seconds < 0 ? '-' : '';
  }
}
