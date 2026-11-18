import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

export interface SegmentOption<T extends string> {
  value: T;
  label: string;
}

@Component({
  selector: 'app-segmented',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="segmented" role="radiogroup" [attr.aria-label]="label()">
      @for (option of options(); track option.value) {
        <button
          type="button"
          role="radio"
          [attr.aria-checked]="option.value === value()"
          [class.on]="option.value === value()"
          (click)="value.set(option.value)">
          {{ option.label }}
        </button>
      }
    </div>
  `,
  styles: `
    :host { display: block; }
    .segmented { display: grid; grid-auto-flow: column; grid-auto-columns: 1fr; gap: 4px; padding: 3px; border: 1px solid var(--border); border-radius: 9px; background: var(--field); }
    button { height: 32px; padding: 0 8px; border: 0; border-radius: 7px; background: transparent; color: var(--muted); font-size: 0.9rem; }
    button:hover:not(:disabled) { color: var(--text); background: rgba(255, 255, 255, 0.04); border-color: transparent; }
    button.on { background: var(--primary); color: #fff; font-weight: 600; }
  `
})
export class SegmentedComponent<T extends string> {
  readonly options = input.required<SegmentOption<T>[]>();
  readonly label = input('');
  readonly value = model.required<T>();
}
