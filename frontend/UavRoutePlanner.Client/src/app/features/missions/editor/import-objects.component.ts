import { ChangeDetectionStrategy, Component, inject, output, signal } from '@angular/core';
import { ImportApi } from '../../../core/api/import.api';
import { errorMessage } from '../../../core/http/error-message';
import { ImportError, ImportedObject } from '../../../core/models';
import { AlertComponent } from '../../../shared/ui/alert.component';

@Component({
  selector: 'app-import-objects',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AlertComponent],
  template: `
    <label class="file">
      Import objects from CSV or GeoJSON
      <input type="file" accept=".csv,.txt,.geojson,.json" (change)="onFile($event)">
    </label>

    <app-alert [message]="error()" />

    @if (problems().length > 0) {
      <ul class="problems">
        @for (problem of problems(); track $index) {
          <li>Row {{ problem.line }}: {{ problem.message }}</li>
        }
      </ul>
    }
  `
})
export class ImportObjectsComponent {
  private readonly api = inject(ImportApi);

  readonly imported = output<ImportedObject[]>();

  protected readonly error = signal<string | null>(null);
  protected readonly problems = signal<ImportError[]>([]);

  protected async onFile(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.error.set(null);
    this.problems.set([]);

    const content = await file.text();
    this.api.importObjects(file.name, content).subscribe({
      next: result => {
        this.problems.set(result.errors);
        if (result.objects.length > 0) this.imported.emit(result.objects);
      },
      error: (e: unknown) => this.error.set(errorMessage(e))
    });

    input.value = '';
  }
}
