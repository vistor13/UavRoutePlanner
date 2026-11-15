import { ChangeDetectionStrategy, Component, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlgorithmSettings } from '../../../core/models';

@Component({
  selector: 'app-algorithm-settings',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
  template: `
    <div class="grid three">
      <label>Alpha (pheromone weight)
        <input type="number" step="any" [ngModel]="settings().alpha" (ngModelChange)="patch('alpha', $event)">
      </label>
      <label>Beta (heuristic weight)
        <input type="number" step="any" [ngModel]="settings().beta" (ngModelChange)="patch('beta', $event)">
      </label>
      <label>Evaporation rate (0..1)
        <input type="number" step="any" [ngModel]="settings().evaporationRate" (ngModelChange)="patch('evaporationRate', $event)">
      </label>
      <label>Pheromone deposit Q
        <input type="number" step="any" [ngModel]="settings().pheromoneDeposit" (ngModelChange)="patch('pheromoneDeposit', $event)">
      </label>
      <label>Ants
        <input type="number" step="1" [ngModel]="settings().antsCount" (ngModelChange)="patch('antsCount', $event)">
      </label>
      <label>Iterations
        <input type="number" step="1" [ngModel]="settings().iterationsCount" (ngModelChange)="patch('iterationsCount', $event)">
      </label>
    </div>
  `
})
export class AlgorithmSettingsComponent {
  readonly settings = model.required<AlgorithmSettings>();

  protected patch(key: keyof AlgorithmSettings, value: number): void {
    this.settings.update(current => ({ ...current, [key]: Number(value) }));
  }
}
