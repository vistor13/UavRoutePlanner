import { ChangeDetectionStrategy, Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MissionApi } from '../../core/api/mission.api';
import { UavProfileApi } from '../../core/api/uav-profile.api';
import { errorMessage } from '../../core/http/error-message';
import { MissionSummary, UavProfile } from '../../core/models';
import { AlertComponent } from '../../shared/ui/alert.component';

type SortKey = 'name' | 'createdAt' | 'uav' | 'objectsCount' | 'routesCount' | 'best';
type SortDirection = 'asc' | 'desc';

interface Point {
  x: number;
  y: number;
}

const PAGE_SIZES = [5, 10, 20];

@Component({
  selector: 'app-missions',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe, AlertComponent],
  templateUrl: './missions.component.html',
  styleUrl: './missions.component.css',
  host: { '(document:keydown.escape)': 'cancelDelete()' }
})
export class MissionsComponent implements OnInit {
  private readonly api = inject(MissionApi);
  private readonly profilesApi = inject(UavProfileApi);
  private readonly cancelButton = viewChild<ElementRef<HTMLButtonElement>>('cancelButton');

  protected readonly pageSizes = PAGE_SIZES;
  protected readonly missions = signal<MissionSummary[]>([]);
  protected readonly loaded = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly profiles = signal<UavProfile[]>([]);
  protected readonly uavFilter = signal('all');
  protected readonly query = signal('');
  protected readonly sortKey = signal<SortKey>('createdAt');
  protected readonly sortDirection = signal<SortDirection>('desc');
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly pendingDelete = signal<MissionSummary | null>(null);
  protected readonly deleting = signal(false);

  protected readonly profileNames = computed(() => new Map(this.profiles().map(profile => [profile.id, profile.name])));
  protected readonly hasUnassigned = computed(() => this.missions().some(mission => mission.uavProfileId === null));

  protected readonly scoped = computed(() => {
    const filter = this.uavFilter();
    if (filter === 'all') {
      return this.missions();
    }
    if (filter === 'none') {
      return this.missions().filter(mission => mission.uavProfileId === null);
    }
    return this.missions().filter(mission => mission.uavProfileId === Number(filter));
  });

  protected readonly totalRoutes = computed(() => this.scoped().reduce((sum, m) => sum + m.routesCount, 0));
  protected readonly fullyCovered = computed(() => this.scoped().filter(mission => this.complete(mission)).length);

  protected readonly filtered = computed(() => {
    const term = this.query().trim().toLowerCase();
    const list = term ? this.scoped().filter(m => m.name.toLowerCase().includes(term)) : this.scoped();
    const key = this.sortKey();
    const factor = this.sortDirection() === 'asc' ? 1 : -1;
    return [...list].sort((a, b) => this.compare(a, b, key) * factor);
  });

  protected readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / this.pageSize())));
  protected readonly currentPage = computed(() => Math.min(this.page(), this.totalPages()));
  protected readonly pageItems = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filtered().slice(start, start + this.pageSize());
  });
  protected readonly pageNumbers = computed(() => Array.from({ length: this.totalPages() }, (_, i) => i + 1));

  ngOnInit(): void {
    this.load();
    this.profilesApi.getAll().subscribe({
      next: profiles => this.profiles.set(profiles),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }

  protected uavName(mission: MissionSummary): string | null {
    if (mission.uavProfileId === null) {
      return null;
    }
    return this.profileNames().get(mission.uavProfileId) ?? `UAV #${mission.uavProfileId}`;
  }

  protected setUavFilter(event: Event): void {
    this.uavFilter.set((event.target as HTMLSelectElement).value);
    this.page.set(1);
  }

  protected onSearch(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
    this.page.set(1);
  }

  protected sortBy(key: SortKey): void {
    if (this.sortKey() === key) {
      this.sortDirection.update(direction => (direction === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortKey.set(key);
      this.sortDirection.set(key === 'name' ? 'asc' : 'desc');
    }
    this.page.set(1);
  }

  protected ariaSort(key: SortKey): 'ascending' | 'descending' | 'none' {
    if (this.sortKey() !== key) {
      return 'none';
    }
    return this.sortDirection() === 'asc' ? 'ascending' : 'descending';
  }

  protected goTo(page: number): void {
    this.page.set(Math.min(Math.max(1, page), this.totalPages()));
  }

  protected setPageSize(event: Event): void {
    this.pageSize.set(Number((event.target as HTMLSelectElement).value));
    this.page.set(1);
  }

  protected percent(mission: MissionSummary): number {
    return Math.round(this.ratio(mission) * 100);
  }

  protected complete(mission: MissionSummary): boolean {
    return mission.objectsCount > 0 && mission.bestVisitedObjectsCount === mission.objectsCount;
  }

  protected thumbnail(mission: MissionSummary): Point[] {
    let seed = mission.id * 9301 + 49297;
    const next = () => {
      seed = (seed * 9301 + 49297) % 233280;
      return seed / 233280;
    };
    const count = Math.min(Math.max(mission.objectsCount, 3), 6);
    return Array.from({ length: count }, (_, i) => ({
      x: 12 + (i / (count - 1)) * 96 + (next() - 0.5) * 10,
      y: 14 + next() * 42
    }));
  }

  protected points(path: Point[]): string {
    return path.map(point => `${point.x.toFixed(1)},${point.y.toFixed(1)}`).join(' ');
  }

  protected askDelete(mission: MissionSummary): void {
    this.pendingDelete.set(mission);
    setTimeout(() => this.cancelButton()?.nativeElement.focus());
  }

  protected cancelDelete(): void {
    if (!this.deleting()) {
      this.pendingDelete.set(null);
    }
  }

  protected confirmDelete(): void {
    const mission = this.pendingDelete();
    if (mission === null || this.deleting()) {
      return;
    }
    this.error.set(null);
    this.deleting.set(true);
    this.api.delete(mission.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
        this.load();
      },
      error: (e: unknown) => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
        this.error.set(errorMessage(e));
      }
    });
  }

  private ratio(mission: MissionSummary): number {
    if (mission.bestVisitedObjectsCount === null || mission.objectsCount === 0) {
      return 0;
    }
    return mission.bestVisitedObjectsCount / mission.objectsCount;
  }

  private compare(a: MissionSummary, b: MissionSummary, key: SortKey): number {
    switch (key) {
      case 'name':
        return a.name.localeCompare(b.name, undefined, { numeric: true, sensitivity: 'base' });
      case 'createdAt':
        return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      case 'uav':
        return (this.uavName(a) ?? '').localeCompare(this.uavName(b) ?? '', undefined, { numeric: true, sensitivity: 'base' });
      case 'objectsCount':
        return a.objectsCount - b.objectsCount;
      case 'routesCount':
        return a.routesCount - b.routesCount;
      case 'best':
        return this.ratio(a) - this.ratio(b);
    }
  }

  private load(): void {
    this.api.getAll().subscribe({
      next: missions => {
        this.missions.set(missions);
        this.loaded.set(true);
      },
      error: (e: unknown) => {
        this.loaded.set(true);
        this.error.set(errorMessage(e));
      }
    });
  }
}
