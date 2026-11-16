import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-logo',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg [attr.width]="size()" [attr.height]="size()" viewBox="0 0 64 64" fill="none" stroke="currentColor"
         stroke-width="4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
      <circle cx="14" cy="14" r="8"/>
      <circle cx="50" cy="14" r="8"/>
      <circle cx="14" cy="50" r="8"/>
      <circle cx="50" cy="50" r="8"/>
      <path d="M20 20 L44 44 M44 20 L20 44"/>
      <circle cx="32" cy="32" r="5" fill="currentColor"/>
    </svg>
  `,
  styles: `:host { display: inline-flex; color: #29b6f6; } svg { display: block; }`
})
export class LogoComponent {
  readonly size = input(32);
}
