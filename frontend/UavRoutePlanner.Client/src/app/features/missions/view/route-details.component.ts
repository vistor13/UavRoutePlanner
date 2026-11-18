import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { RouteLeg, RouteResult } from '../../../core/models';
import { formatDistance, formatDuration } from '../../../core/util/format';
import { RouteExportComponent } from './route-export.component';

const PREVIEW_LEGS = 5;

@Component({
  selector: 'app-route-details',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouteExportComponent],
  templateUrl: './route-details.component.html',
  styleUrl: './route-details.component.css'
})
export class RouteDetailsComponent {
  readonly route = input.required<RouteResult>();
  readonly exportable = input(true);

  protected readonly expanded = signal(false);
  protected readonly duration = formatDuration;
  protected readonly distance = formatDistance;

  protected readonly legs = computed(() => {
    const all = this.route().legs;
    return this.expanded() ? all : all.slice(0, PREVIEW_LEGS);
  });

  protected readonly canExpand = computed(() => this.route().legs.length > PREVIEW_LEGS);

  protected toggle(): void {
    this.expanded.update(value => !value);
  }

  protected from(index: number): string {
    return index === 0 ? 'S' : this.to(this.route().legs[index - 1], index - 1);
  }

  protected to(leg: RouteLeg, index: number): string {
    if (leg.objectNumber !== null) return `${leg.objectNumber}`;
    return index === this.route().legs.length - 1 ? 'F' : 'S';
  }
}
