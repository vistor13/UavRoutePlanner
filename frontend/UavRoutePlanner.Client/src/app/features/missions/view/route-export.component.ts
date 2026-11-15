import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { RouteApi } from '../../../core/api/route.api';
import { errorMessage } from '../../../core/http/error-message';
import { ExportFormat, RouteResult } from '../../../core/models';
import { FileDownloadService } from '../../../core/util/file-download.service';
import { AlertComponent } from '../../../shared/ui/alert.component';

@Component({
  selector: 'app-route-export',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AlertComponent],
  template: `
    <div class="row">
      <span class="muted">Export:</span>
      <button type="button" (click)="download('csv')">CSV</button>
      <button type="button" (click)="download('geojson')">GeoJSON</button>
      <button type="button" (click)="download('waypoints')">Waypoints</button>
    </div>
    <app-alert [message]="error()" />
  `
})
export class RouteExportComponent {
  private readonly api = inject(RouteApi);
  private readonly files = inject(FileDownloadService);

  readonly route = input.required<RouteResult>();
  protected readonly error = signal<string | null>(null);

  protected download(format: ExportFormat): void {
    const route = this.route();
    this.error.set(null);

    this.api.export(route.missionId, route.id, format).subscribe({
      next: response => this.files.save(response, `route_${route.missionId}_${route.id}.${format}`),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }
}
