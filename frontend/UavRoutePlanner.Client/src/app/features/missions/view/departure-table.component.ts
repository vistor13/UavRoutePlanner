import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DepartureHour, DepartureRecommendation } from '../../../core/models';
import { formatDuration, formatLocalHour, zoneLabel } from '../../../core/util/format';
import { WindArrowComponent } from '../../../shared/ui/wind-arrow.component';

@Component({
  selector: 'app-departure-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WindArrowComponent],
  templateUrl: './departure-table.component.html',
  styleUrl: './departure-table.component.css'
})
export class DepartureTableComponent {
  readonly recommendation = input.required<DepartureRecommendation>();
  readonly total = input.required<number>();
  readonly selected = input<string | null>(null);

  readonly picked = output<DepartureHour>();

  protected readonly duration = formatDuration;
  protected readonly hour = formatLocalHour;
  protected readonly zone = zoneLabel();

  protected feasible(item: DepartureHour): boolean {
    return item.errorCode === null;
  }

  protected reason(item: DepartureHour): string {
    return item.errorCode === 'TimeBudgetTooSmall' ? 'Time limit exceeded' : 'Not feasible';
  }

  protected pick(item: DepartureHour): void {
    if (this.feasible(item)) this.picked.emit(item);
  }
}
