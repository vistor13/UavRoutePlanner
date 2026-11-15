import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-alert',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `@if (message()) { <div class="alert" [class.alert-info]="kind() === 'info'">{{ message() }}</div> }`
})
export class AlertComponent {
  readonly message = input<string | null>(null);
  readonly kind = input<'error' | 'info'>('error');
}
