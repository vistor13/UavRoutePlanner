import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UavProfileApi } from '../../core/api/uav-profile.api';
import { errorMessage } from '../../core/http/error-message';
import { UavProfile, UavProfileInput } from '../../core/models';
import { AlertComponent } from '../../shared/ui/alert.component';

const EMPTY_FORM: UavProfileInput = {
  name: '',
  airspeedMs: 15,
  maxFlightTimeMin: 20,
  reservePercent: 20,
  flightAltitudeM: 100
};

@Component({
  selector: 'app-profiles',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, AlertComponent],
  template: `
    <h1>UAV profiles</h1>
    <app-alert [message]="error()" />

    <div class="grid two">
      <section class="card">
        <h2>{{ editingId() === null ? 'New profile' : 'Edit profile' }}</h2>
        <form (ngSubmit)="save()">
          <label>Name <input name="name" [(ngModel)]="form.name" required></label>
          <label>Airspeed, m/s <input name="airspeed" type="number" step="any" [(ngModel)]="form.airspeedMs"></label>
          <label>Max flight time, min <input name="time" type="number" step="any" [(ngModel)]="form.maxFlightTimeMin"></label>
          <label>Battery reserve, % <input name="reserve" type="number" step="any" [(ngModel)]="form.reservePercent"></label>
          <label>Flight altitude, m (up to 180) <input name="altitude" type="number" step="any" [(ngModel)]="form.flightAltitudeM"></label>
          <div class="row">
            <button class="primary" type="submit">{{ editingId() === null ? 'Create' : 'Save' }}</button>
            @if (editingId() !== null) {
              <button type="button" (click)="reset()">Cancel</button>
            }
          </div>
        </form>
      </section>

      <section class="card">
        <h2>Your profiles</h2>
        @if (profiles().length === 0) {
          <p class="muted">No profiles yet.</p>
        }
        <ul class="list">
          @for (profile of profiles(); track profile.id) {
            <li>
              <div>
                <strong>{{ profile.name }}</strong>
                <div class="muted">
                  {{ profile.airspeedMs }} m/s &middot; {{ profile.maxFlightTimeMin }} min &middot;
                  reserve {{ profile.reservePercent }}% &middot; {{ profile.flightAltitudeM }} m
                </div>
              </div>
              <div class="row">
                <button type="button" (click)="edit(profile)">Edit</button>
                <button class="danger" type="button" (click)="remove(profile)">Delete</button>
              </div>
            </li>
          }
        </ul>
      </section>
    </div>
  `
})
export class ProfilesComponent implements OnInit {
  private readonly api = inject(UavProfileApi);

  protected readonly profiles = signal<UavProfile[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly editingId = signal<number | null>(null);
  protected form: UavProfileInput = { ...EMPTY_FORM };

  ngOnInit(): void {
    this.load();
  }

  protected edit(profile: UavProfile): void {
    this.editingId.set(profile.id);
    this.form = {
      name: profile.name,
      airspeedMs: profile.airspeedMs,
      maxFlightTimeMin: profile.maxFlightTimeMin,
      reservePercent: profile.reservePercent,
      flightAltitudeM: profile.flightAltitudeM
    };
  }

  protected reset(): void {
    this.editingId.set(null);
    this.form = { ...EMPTY_FORM };
  }

  protected save(): void {
    this.error.set(null);
    const id = this.editingId();
    const request = id === null ? this.api.create(this.form) : this.api.update(id, this.form);

    request.subscribe({
      next: () => {
        this.reset();
        this.load();
      },
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }

  protected remove(profile: UavProfile): void {
    this.error.set(null);
    this.api.delete(profile.id).subscribe({
      next: () => this.load(),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }

  private load(): void {
    this.api.getAll().subscribe({
      next: profiles => this.profiles.set(profiles),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }
}
