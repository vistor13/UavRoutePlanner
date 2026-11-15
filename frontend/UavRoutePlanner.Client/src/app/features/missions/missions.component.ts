import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MissionApi } from '../../core/api/mission.api';
import { errorMessage } from '../../core/http/error-message';
import { MissionSummary } from '../../core/models';
import { AlertComponent } from '../../shared/ui/alert.component';

@Component({
  selector: 'app-missions',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe, AlertComponent],
  template: `
    <div class="row between">
      <h1>Missions</h1>
      <a class="button primary" routerLink="/missions/new">New mission</a>
    </div>

    <app-alert [message]="error()" />

    @if (missions().length === 0) {
      <p class="muted">No missions yet. Create the first one.</p>
    } @else {
      <section class="card">
        <table>
          <thead>
            <tr>
              <th>Name</th><th>Created</th><th>Objects</th><th>Routes</th><th>Best result</th><th></th>
            </tr>
          </thead>
          <tbody>
            @for (mission of missions(); track mission.id) {
              <tr>
                <td><a [routerLink]="['/missions', mission.id]">{{ mission.name }}</a></td>
                <td>{{ mission.createdAt | date: 'dd.MM.yyyy HH:mm' }}</td>
                <td>{{ mission.objectsCount }}</td>
                <td>{{ mission.routesCount }}</td>
                <td>
                  @if (mission.bestVisitedObjectsCount !== null) {
                    {{ mission.bestVisitedObjectsCount }} of {{ mission.objectsCount }}
                  } @else {
                    &mdash;
                  }
                </td>
                <td class="right">
                  <button class="danger" type="button" (click)="remove(mission)">Delete</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </section>
    }
  `
})
export class MissionsComponent implements OnInit {
  private readonly api = inject(MissionApi);

  protected readonly missions = signal<MissionSummary[]>([]);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected remove(mission: MissionSummary): void {
    this.error.set(null);
    this.api.delete(mission.id).subscribe({
      next: () => this.load(),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }

  private load(): void {
    this.api.getAll().subscribe({
      next: missions => this.missions.set(missions),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }
}
