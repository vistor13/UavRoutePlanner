import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { RouteResult } from '../../../core/models';

const PREVIEW_ROUTES = 5;

@Component({
  selector: 'app-route-history',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  templateUrl: './route-history.component.html',
  styleUrl: './route-history.component.css'
})
export class RouteHistoryComponent {
  readonly routes = input.required<RouteResult[]>();
  readonly selectedId = input<number | null>(null);

  readonly picked = output<RouteResult>();

  protected readonly expanded = signal(false);

  protected readonly ordered = computed(() => [...this.routes()].reverse());
  protected readonly visible = computed(() => (this.expanded() ? this.ordered() : this.ordered().slice(0, PREVIEW_ROUTES)));
  protected readonly canExpand = computed(() => this.routes().length > PREVIEW_ROUTES);

  protected toggle(): void {
    this.expanded.update(value => !value);
  }

  protected wind(route: RouteResult): string {
    return route.windMode === 'NoWind'
      ? 'No wind'
      : `${route.windSpeedMs.toFixed(1)} m/s from ${route.windDirectionFromDeg.toFixed(0)}°`;
  }

  protected complete(route: RouteResult): boolean {
    return route.visitedObjectsCount === route.totalObjectsCount;
  }
}
