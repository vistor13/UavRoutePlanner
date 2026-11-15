import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-wind-arrow',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="wind">
      <span class="wind-arrow" [style.transform]="'rotate(' + rotation() + 'deg)'">&#8593;</span>
      {{ speedMs().toFixed(1) }} m/s from {{ directionFromDeg().toFixed(0) }}&deg;
    </span>
  `,
  styles: `
    .wind { display: inline-flex; align-items: center; gap: 6px; }
    .wind-arrow { display: inline-block; font-weight: 700; color: var(--primary); }
  `
})
export class WindArrowComponent {
  readonly speedMs = input.required<number>();
  readonly directionFromDeg = input.required<number>();
  protected readonly rotation = computed(() => (this.directionFromDeg() + 180) % 360);
}
