import { ChangeDetectionStrategy, Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, of, startWith, switchMap, tap } from 'rxjs';
import { MissionApi } from '../../core/api/mission.api';
import { UavProfileApi } from '../../core/api/uav-profile.api';
import { errorMessage } from '../../core/http/error-message';
import { MissionSummary, UAV_LIMITS, UavProfile, UavProfileInput } from '../../core/models';
import { MAX_SOURCE_BYTES, toSquarePhoto } from '../../core/util/image-resize';
import { ProfilePhotoCache } from '../../core/util/profile-photo.cache';
import { AlertComponent } from '../../shared/ui/alert.component';
import { UavPhotoComponent } from '../../shared/ui/uav-photo/uav-photo.component';

type FieldName = 'name' | 'airspeedMs' | 'maxFlightTimeMin' | 'reservePercent' | 'flightAltitudeM';

interface PendingPhoto {
  blob: Blob;
  url: string;
}

const DEFAULTS = {
  name: '',
  airspeedMs: 15 as number | null,
  maxFlightTimeMin: 20 as number | null,
  reservePercent: 20 as number | null,
  flightAltitudeM: 100 as number | null
};

@Component({
  selector: 'app-profiles',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, AlertComponent, UavPhotoComponent],
  templateUrl: './profiles.component.html',
  styleUrl: './profiles.component.css',
  host: { '(document:keydown.escape)': 'onEscape()' }
})
export class ProfilesComponent implements OnInit {
  private readonly api = inject(UavProfileApi);
  private readonly missionApi = inject(MissionApi);
  private readonly photos = inject(ProfilePhotoCache);
  private readonly nameField = viewChild<ElementRef<HTMLInputElement>>('nameField');
  private readonly cancelButton = viewChild<ElementRef<HTMLButtonElement>>('cancelButton');

  protected readonly limits = UAV_LIMITS;

  protected readonly form = new FormGroup({
    name: new FormControl(DEFAULTS.name, {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(UAV_LIMITS.nameMaxLength), this.notBlank]
    }),
    airspeedMs: new FormControl(DEFAULTS.airspeedMs, [
      Validators.required,
      Validators.min(0.1),
      Validators.max(UAV_LIMITS.airspeedMaxMs)
    ]),
    maxFlightTimeMin: new FormControl(DEFAULTS.maxFlightTimeMin, [
      Validators.required,
      Validators.min(1),
      Validators.max(UAV_LIMITS.flightTimeMaxMin)
    ]),
    reservePercent: new FormControl(DEFAULTS.reservePercent, [
      Validators.required,
      Validators.min(0),
      Validators.max(UAV_LIMITS.reserveMaxPercent)
    ]),
    flightAltitudeM: new FormControl(DEFAULTS.flightAltitudeM, [
      Validators.required,
      Validators.min(1),
      Validators.max(UAV_LIMITS.altitudeMaxM)
    ])
  });

  private readonly formValue = toSignal(this.form.valueChanges.pipe(startWith(this.form.getRawValue())), {
    initialValue: this.form.getRawValue()
  });

  protected readonly profiles = signal<UavProfile[]>([]);
  protected readonly missions = signal<MissionSummary[]>([]);
  protected readonly loaded = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly query = signal('');

  protected readonly panelOpen = signal(false);
  protected readonly editingId = signal<number | null>(null);
  protected readonly saving = signal(false);
  protected readonly submitted = signal(false);
  protected readonly panelError = signal<string | null>(null);
  protected readonly currentVersion = signal<number | null>(null);
  protected readonly pendingPhoto = signal<PendingPhoto | null>(null);
  protected readonly photoRemoved = signal(false);
  protected readonly photoBusy = signal(false);

  protected readonly pendingDelete = signal<UavProfile | null>(null);
  protected readonly deleting = signal(false);

  protected readonly visible = computed(() => {
    const term = this.query().trim().toLowerCase();
    return term ? this.profiles().filter(profile => profile.name.toLowerCase().includes(term)) : this.profiles();
  });

  protected readonly usage = computed(() => {
    const counts = new Map<number, number>();
    for (const mission of this.missions()) {
      if (mission.uavProfileId !== null) {
        counts.set(mission.uavProfileId, (counts.get(mission.uavProfileId) ?? 0) + 1);
      }
    }
    return counts;
  });

  protected readonly longestFlight = computed(() =>
    this.profiles().reduce((max, profile) => Math.max(max, profile.maxFlightTimeMin), 0)
  );

  protected readonly fastest = computed(() =>
    this.profiles().reduce((max, profile) => Math.max(max, profile.airspeedMs), 0)
  );

  protected readonly formUsable = computed(() => {
    const { maxFlightTimeMin, reservePercent } = this.formValue();
    if (maxFlightTimeMin === null || maxFlightTimeMin === undefined || maxFlightTimeMin <= 0) {
      return null;
    }
    if (reservePercent === null || reservePercent === undefined || reservePercent < 0 || reservePercent >= 100) {
      return null;
    }
    return this.usable(maxFlightTimeMin, reservePercent);
  });

  protected readonly formMax = computed(() => this.formValue().maxFlightTimeMin ?? 0);

  protected readonly hasPhoto = computed(
    () => this.pendingPhoto() !== null || (this.currentVersion() !== null && !this.photoRemoved())
  );

  protected readonly panelVersion = computed(() => (this.photoRemoved() ? null : this.currentVersion()));

  ngOnInit(): void {
    this.load();
    this.missionApi.getAll().subscribe({
      next: missions => this.missions.set(missions),
      error: (e: unknown) => this.error.set(errorMessage(e))
    });
  }

  protected onSearch(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  protected usable(maxFlightTimeMin: number, reservePercent: number): number {
    return Math.round(maxFlightTimeMin * (1 - reservePercent / 100) * 10) / 10;
  }

  protected usableShare(profile: UavProfile): number {
    const longest = this.longestFlight();
    if (longest <= 0) {
      return 0;
    }
    return Math.min(100, (this.usable(profile.maxFlightTimeMin, profile.reservePercent) / longest) * 100);
  }

  protected usableShareOfForm(): number {
    const usable = this.formUsable();
    const max = this.formMax();
    return usable === null || max <= 0 ? 0 : Math.min(100, (usable / max) * 100);
  }

  protected usageLabel(profile: UavProfile): string {
    const count = this.usage().get(profile.id) ?? 0;
    if (count === 0) {
      return 'Not used yet';
    }
    return count === 1 ? 'Used in 1 mission' : `Used in ${count} missions`;
  }

  protected deleteText(profile: UavProfile): string {
    const count = this.usage().get(profile.id) ?? 0;
    if (count === 0) {
      return `“${profile.name}” will be removed. This action cannot be undone.`;
    }
    const missions = count === 1 ? '1 mission uses' : `${count} missions use`;
    return `“${profile.name}” will be removed. ${missions} it and will be left without a UAV profile.`;
  }

  protected openCreate(): void {
    this.resetPanel();
    this.form.reset(DEFAULTS);
    this.panelOpen.set(true);
    this.focusName();
  }

  protected openEdit(profile: UavProfile): void {
    this.resetPanel();
    this.form.reset({
      name: profile.name,
      airspeedMs: profile.airspeedMs,
      maxFlightTimeMin: profile.maxFlightTimeMin,
      reservePercent: profile.reservePercent,
      flightAltitudeM: profile.flightAltitudeM
    });
    this.editingId.set(profile.id);
    this.currentVersion.set(profile.photoVersion);
    this.panelOpen.set(true);
    this.focusName();
  }

  protected closePanel(): void {
    if (this.saving()) {
      return;
    }
    this.discardPending();
    this.panelOpen.set(false);
  }

  protected invalid(name: FieldName): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected async onFile(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    this.panelError.set(null);
    if (!file.type.startsWith('image/')) {
      this.panelError.set('Choose an image file (JPG, PNG or WebP).');
      return;
    }
    if (file.size > MAX_SOURCE_BYTES) {
      this.panelError.set('The image is too large. Choose a file up to 15 MB.');
      return;
    }

    this.photoBusy.set(true);
    try {
      const blob = await toSquarePhoto(file);
      this.discardPending();
      this.pendingPhoto.set({ blob, url: URL.createObjectURL(blob) });
      this.photoRemoved.set(false);
    } catch {
      this.panelError.set('This image could not be read. Try a JPG, PNG or WebP file.');
    } finally {
      this.photoBusy.set(false);
    }
  }

  protected removePhoto(): void {
    this.discardPending();
    if (this.currentVersion() !== null) {
      this.photoRemoved.set(true);
    }
  }

  protected save(): void {
    this.submitted.set(true);
    this.panelError.set(null);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving() || this.photoBusy()) {
      return;
    }

    const value = this.form.getRawValue();
    const input: UavProfileInput = {
      name: value.name.trim(),
      airspeedMs: Number(value.airspeedMs),
      maxFlightTimeMin: Number(value.maxFlightTimeMin),
      reservePercent: Number(value.reservePercent),
      flightAltitudeM: Number(value.flightAltitudeM)
    };

    const id = this.editingId();
    const request = id === null ? this.api.create(input) : this.api.update(id, input);

    this.saving.set(true);
    request
      .pipe(
        tap(profile => {
          if (id === null) {
            this.editingId.set(profile.id);
            this.load();
          }
        }),
        switchMap(profile => this.applyPhoto(profile))
      )
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.discardPending();
          this.panelOpen.set(false);
          this.load();
        },
        error: (e: unknown) => {
          this.saving.set(false);
          this.panelError.set(errorMessage(e));
        }
      });
  }

  protected askDelete(profile: UavProfile): void {
    this.pendingDelete.set(profile);
    setTimeout(() => this.cancelButton()?.nativeElement.focus());
  }

  protected cancelDelete(): void {
    if (!this.deleting()) {
      this.pendingDelete.set(null);
    }
  }

  protected confirmDelete(): void {
    const profile = this.pendingDelete();
    if (profile === null || this.deleting()) {
      return;
    }

    this.error.set(null);
    this.deleting.set(true);
    this.api.delete(profile.id).subscribe({
      next: () => {
        this.photos.forget(profile.id);
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

  protected onEscape(): void {
    if (this.pendingDelete() !== null) {
      this.cancelDelete();
    } else if (this.panelOpen()) {
      this.closePanel();
    }
  }

  private applyPhoto(profile: UavProfile): Observable<UavProfile> {
    const pending = this.pendingPhoto();
    if (pending) {
      return this.api.uploadPhoto(profile.id, pending.blob);
    }
    if (this.photoRemoved() && profile.photoVersion !== null) {
      return this.api.deletePhoto(profile.id);
    }
    return of(profile);
  }

  private resetPanel(): void {
    this.discardPending();
    this.editingId.set(null);
    this.currentVersion.set(null);
    this.photoRemoved.set(false);
    this.submitted.set(false);
    this.panelError.set(null);
  }

  private discardPending(): void {
    const pending = this.pendingPhoto();
    if (pending) {
      URL.revokeObjectURL(pending.url);
      this.pendingPhoto.set(null);
    }
  }

  private focusName(): void {
    setTimeout(() => this.nameField()?.nativeElement.focus(), 60);
  }

  private notBlank(control: AbstractControl): { blank: true } | null {
    return String(control.value ?? '').trim().length === 0 ? { blank: true } : null;
  }

  private load(): void {
    this.api.getAll().subscribe({
      next: profiles => {
        this.profiles.set(profiles);
        this.loaded.set(true);
      },
      error: (e: unknown) => {
        this.loaded.set(true);
        this.error.set(errorMessage(e));
      }
    });
  }
}
