import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouteResult } from '../../../core/models';
import { formatLocalDateTime, zoneLabel } from '../../../core/util/format';
import { WindArrowComponent } from '../../../shared/ui/wind-arrow.component';

@Component({
  selector: 'app-route-summary',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WindArrowComponent],
  templateUrl: './route-summary.component.html',
  styleUrl: './route-summary.component.css'
})
export class RouteSummaryComponent {
  readonly route = input.required<RouteResult>();

  protected readonly objectsPercent = computed(() => {
    const r = this.route();
    return r.totalObjectsCount === 0 ? 0 : Math.round((r.visitedObjectsCount / r.totalObjectsCount) * 100);
  });

  protected readonly timeMinutes = computed(() => Math.round(this.route().totalTimeS / 60));
  protected readonly limitMinutes = computed(() => Math.round(this.route().timeLimitS / 60));

  protected readonly budgetPercent = computed(() => {
    const r = this.route();
    return r.timeLimitS <= 0 ? 0 : Math.round((r.totalTimeS / r.timeLimitS) * 100);
  });

  protected readonly budgetLevel = computed(() => {
    const percent = this.budgetPercent();
    return percent > 100 ? 'over' : percent >= 85 ? 'high' : 'ok';
  });

  protected readonly complete = computed(() => this.route().visitedObjectsCount === this.route().totalObjectsCount);

  protected readonly departure = computed(() => formatLocalDateTime(this.route().departureTimeUtc));
  protected readonly zone = computed(() => zoneLabel(this.route().departureTimeUtc));
}
