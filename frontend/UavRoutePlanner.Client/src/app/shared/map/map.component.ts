import {
  AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, OnDestroy, effect, input, output, viewChild
} from '@angular/core';
import * as L from 'leaflet';

export type MarkerKind = 'start' | 'finish' | 'object' | 'visited' | 'skipped';

export interface MapMarker {
  id: string;
  latitude: number;
  longitude: number;
  label: string;
  kind: MarkerKind;
}

export interface MapClick {
  latitude: number;
  longitude: number;
}

const DEFAULT_CENTER: L.LatLngTuple = [50.45, 30.52];
const DEFAULT_ZOOM = 12;

@Component({
  selector: 'app-map',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div #host class="map"></div>`,
  styles: `
    :host { display: block; isolation: isolate; }
    .map { height: 100%; min-height: 420px; border-radius: 8px; border: 1px solid var(--border); }
  `
})
export class MapComponent implements AfterViewInit, OnDestroy {
  readonly markers = input<MapMarker[]>([]);
  readonly path = input<[number, number][]>([]);
  readonly autoFit = input(false);

  readonly mapClicked = output<MapClick>();
  readonly markerClicked = output<string>();

  private readonly host = viewChild.required<ElementRef<HTMLDivElement>>('host');
  private map?: L.Map;
  private readonly layer = L.layerGroup();

  constructor() {
    effect(() => {
      this.markers();
      this.path();
      this.render();
    });
  }

  ngAfterViewInit(): void {
    this.map = L.map(this.host().nativeElement).setView(DEFAULT_CENTER, DEFAULT_ZOOM);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    this.layer.addTo(this.map);
    this.map.on('click', (event: L.LeafletMouseEvent) =>
      this.mapClicked.emit({ latitude: event.latlng.lat, longitude: event.latlng.lng }));

    this.render();
    this.fit();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private render(): void {
    if (!this.map) return;

    this.layer.clearLayers();

    const path = this.path();
    if (path.length > 1) {
      L.polyline(path, { color: '#2563eb', weight: 4, opacity: 0.85 }).addTo(this.layer);
    }

    for (const marker of this.markers()) {
      const icon = L.divIcon({
        className: '',
        html: `<div class="pin pin-${marker.kind}">${marker.label}</div>`,
        iconSize: [28, 28],
        iconAnchor: [14, 14]
      });

      L.marker([marker.latitude, marker.longitude], { icon })
        .on('click', () => this.markerClicked.emit(marker.id))
        .addTo(this.layer);
    }

    if (this.autoFit()) this.fit();
  }

  private fit(): void {
    if (!this.map) return;

    const points: L.LatLngTuple[] = [
      ...this.markers().map(m => [m.latitude, m.longitude] as L.LatLngTuple),
      ...this.path()
    ];

    if (points.length > 0) {
      this.map.fitBounds(L.latLngBounds(points), { padding: [40, 40], maxZoom: 16 });
    }
  }
}
