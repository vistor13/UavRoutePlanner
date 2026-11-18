import { ChangeDetectionStrategy, Component, input } from '@angular/core';

let nextId = 0;

@Component({
  selector: 'app-hint',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button type="button" class="icon" [attr.aria-describedby]="id" aria-label="More information">i</button>
    <span class="tip" role="tooltip" [id]="id">{{ text() }}</span>
  `,
  styleUrl: './hint.component.css'
})
export class HintComponent {
  readonly text = input.required<string>();
  protected readonly id = `hint-${nextId++}`;
}
