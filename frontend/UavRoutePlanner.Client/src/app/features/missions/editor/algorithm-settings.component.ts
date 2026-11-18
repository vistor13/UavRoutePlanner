import { ChangeDetectionStrategy, Component, computed, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlgorithmSettings, DEFAULT_ALGORITHM } from '../../../core/models';
import { HintComponent } from '../../../shared/ui/hint/hint.component';

interface Field {
  key: keyof AlgorithmSettings;
  label: string;
  hint: string;
  step: string;
  min: number;
  max: number;
  integer: boolean;
}

export const ALGORITHM_FIELDS: Field[] = [
  {
    key: 'alpha', label: 'Alpha', step: '0.1', min: 0, max: 10, integer: false,
    hint: 'How strongly ants follow pheromone trails left by previous ants. Higher values make the search converge faster but can get stuck on a mediocre route.'
  },
  {
    key: 'beta', label: 'Beta', step: '0.1', min: 0, max: 10, integer: false,
    hint: 'How strongly ants prefer closer objects. Higher values make routes greedier and shorter at each step, with less exploration.'
  },
  {
    key: 'evaporationRate', label: 'Evaporation rate', step: '0.01', min: 0.01, max: 1, integer: false,
    hint: 'How quickly old pheromone trails fade each iteration. Higher values forget faster and explore more; lower values keep good routes longer.'
  },
  {
    key: 'pheromoneDeposit', label: 'Pheromone deposit Q', step: '0.01', min: 0.001, max: 100, integer: false,
    hint: 'The amount of pheromone an ant leaves on its route. Shorter routes receive a stronger trail.'
  },
  {
    key: 'antsCount', label: 'Ants', step: '1', min: 1, max: 200, integer: true,
    hint: 'Number of ants (candidate routes) built in each iteration. More ants give better results but take longer to compute.'
  },
  {
    key: 'iterationsCount', label: 'Iterations', step: '1', min: 1, max: 1000, integer: true,
    hint: 'How many times the colony repeats the search. More iterations improve quality up to a point, then only cost time.'
  }
];

export function isAlgorithmValid(settings: AlgorithmSettings): boolean {
  return ALGORITHM_FIELDS.every(field => fieldError(field, settings[field.key]) === null);
}

function fieldError(field: Field, value: number): string | null {
  if (!Number.isFinite(value)) return 'Enter a number';
  if (field.integer && !Number.isInteger(value)) return 'Whole number only';
  if (value < field.min || value > field.max) return `Allowed: ${field.min} to ${field.max}`;
  return null;
}

@Component({
  selector: 'app-algorithm-settings',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, HintComponent],
  templateUrl: './algorithm-settings.component.html',
  styleUrl: './algorithm-settings.component.css'
})
export class AlgorithmSettingsComponent {
  readonly settings = model.required<AlgorithmSettings>();

  protected readonly open = signal(false);
  protected readonly fields = ALGORITHM_FIELDS;
  protected readonly defaults = DEFAULT_ALGORITHM;

  protected readonly isDefault = computed(() =>
    ALGORITHM_FIELDS.every(field => this.settings()[field.key] === DEFAULT_ALGORITHM[field.key]));

  protected readonly errors = computed(() => {
    const settings = this.settings();
    const result: Partial<Record<keyof AlgorithmSettings, string>> = {};
    for (const field of ALGORITHM_FIELDS) {
      const error = fieldError(field, settings[field.key]);
      if (error) result[field.key] = error;
    }
    return result;
  });

  protected toggle(): void {
    this.open.update(value => !value);
  }

  protected patch(key: keyof AlgorithmSettings, value: number | string | null): void {
    const parsed = value === null || value === '' ? Number.NaN : Number(value);
    this.settings.update(current => ({ ...current, [key]: parsed }));
  }

  protected reset(): void {
    this.settings.set({ ...DEFAULT_ALGORITHM });
  }
}
