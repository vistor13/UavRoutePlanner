import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-steps',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <ol class="steps" aria-label="Progress">
      @for (item of items; track item.number) {
        @if (!$first) {
          <li class="line" aria-hidden="true"></li>
        }
        <li [class.done]="item.number < step()" [class.current]="item.number === step()" [attr.aria-current]="item.number === step() ? 'step' : null">
          <span class="dot">
            @if (item.number < step()) {
              <svg viewBox="0 0 20 20" width="14" height="14" aria-hidden="true"><path d="M5 10.4l3.2 3.2L15 6.8" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/></svg>
            } @else {
              {{ item.number }}
            }
          </span>
          @if (item.number === 1 && missionLink(); as link) {
            <a class="name link" [routerLink]="link">{{ item.label }}</a>
          } @else {
            <span class="name">{{ item.label }}</span>
          }
        </li>
      }
    </ol>
  `,
  styles: `
    :host { display: block; }
    .steps { display: flex; align-items: center; gap: 12px; margin: 0; padding: 0; list-style: none; }
    li { display: flex; align-items: center; gap: 8px; color: var(--muted); font-size: 0.95rem; }
    .dot { width: 28px; height: 28px; display: inline-flex; align-items: center; justify-content: center; border: 1px solid #3a5384; border-radius: 50%; font-size: 0.85rem; }
    .line { width: 44px; height: 1px; background: #3a5384; }
    .done { color: var(--text); }
    .done .dot { border-color: #2f9e5f; background: rgba(47, 158, 95, 0.2); color: #6fe3a1; }
    .current { color: #6fb1ff; font-weight: 600; }
    .current .dot { border-color: var(--primary); background: var(--primary); color: #fff; }
    .link { color: inherit; text-decoration: none; }
    .link:hover { text-decoration: underline; }
  `
})
export class StepsComponent {
  readonly step = input.required<1 | 2 | 3>();
  readonly missionLink = input<unknown[] | null>(null);

  protected readonly items = [
    { number: 1, label: 'Mission' },
    { number: 2, label: 'Flight settings' },
    { number: 3, label: 'Route' }
  ];
}
