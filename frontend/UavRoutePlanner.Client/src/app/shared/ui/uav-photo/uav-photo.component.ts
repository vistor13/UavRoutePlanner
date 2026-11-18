import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { ProfilePhotoCache } from '../../../core/util/profile-photo.cache';

@Component({
  selector: 'app-uav-photo',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (shown(); as url) {
      <img [src]="url" alt="" draggable="false">
    } @else {
      <svg viewBox="0 0 48 48" aria-hidden="true">
        <circle cx="10" cy="10" r="6"/>
        <circle cx="38" cy="10" r="6"/>
        <circle cx="10" cy="38" r="6"/>
        <circle cx="38" cy="38" r="6"/>
        <path d="M14 14l6 6M34 14l-6 6M14 34l6-6M34 34l-6-6"/>
        <rect x="19" y="19" width="10" height="10" rx="3"/>
      </svg>
    }
  `,
  styles: `
    :host {
      display: grid;
      place-items: center;
      width: 100%;
      height: 100%;
      overflow: hidden;
      color: var(--accent);
    }

    img {
      display: block;
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    svg {
      width: 58%;
      height: 58%;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.8;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
  `
})
export class UavPhotoComponent {
  private readonly cache = inject(ProfilePhotoCache);

  readonly profileId = input<number | null>(null);
  readonly version = input<number | null>(null);
  readonly previewUrl = input<string | null>(null);

  private readonly loaded = signal<string | null>(null);

  protected readonly shown = computed(() => this.previewUrl() ?? this.loaded());

  constructor() {
    effect(onCleanup => {
      const id = this.profileId();
      const version = this.version();
      if (id === null || version === null) {
        this.loaded.set(null);
        return;
      }

      const subscription = this.cache.url(id, version).subscribe({
        next: url => this.loaded.set(url),
        error: () => this.loaded.set(null)
      });
      onCleanup(() => subscription.unsubscribe());
    });
  }
}
