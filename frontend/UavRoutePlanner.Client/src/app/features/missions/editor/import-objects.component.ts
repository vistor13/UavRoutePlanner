import { ChangeDetectionStrategy, Component, ElementRef, inject, input, output, signal, viewChild } from '@angular/core';
import { ImportApi } from '../../../core/api/import.api';
import { errorMessage } from '../../../core/http/error-message';
import { ImportError, ImportedObject } from '../../../core/models';
import { AlertComponent } from '../../../shared/ui/alert.component';

export interface ImportedFile {
  id: number;
  name: string;
  count: number;
}

export interface ImportResult {
  name: string;
  objects: ImportedObject[];
}

@Component({
  selector: 'app-import-objects',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AlertComponent],
  templateUrl: './import-objects.component.html',
  styleUrl: './import-objects.component.css'
})
export class ImportObjectsComponent {
  private readonly api = inject(ImportApi);
  private readonly picker = viewChild.required<ElementRef<HTMLInputElement>>('picker');

  readonly files = input<ImportedFile[]>([]);
  readonly imported = output<ImportResult>();
  readonly removed = output<number>();

  protected readonly open = signal(false);
  protected readonly dragging = signal(false);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly problems = signal<ImportError[]>([]);

  protected toggle(): void {
    this.open.update(value => !value);
  }

  protected browse(): void {
    this.picker().nativeElement.click();
  }

  protected onPick(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (file) void this.read(file);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(true);
  }

  protected onDragLeave(): void {
    this.dragging.set(false);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) void this.read(file);
  }

  private async read(file: File): Promise<void> {
    this.error.set(null);
    this.problems.set([]);
    this.busy.set(true);

    const content = await file.text();
    this.api.importObjects(file.name, content).subscribe({
      next: result => {
        this.busy.set(false);
        this.problems.set(result.errors);
        if (result.objects.length > 0) this.imported.emit({ name: file.name, objects: result.objects });
        else if (result.errors.length === 0) this.error.set('No objects found in this file.');
      },
      error: (e: unknown) => {
        this.busy.set(false);
        this.error.set(errorMessage(e));
      }
    });
  }
}
