import { Injectable, inject } from '@angular/core';
import { Observable, map, shareReplay } from 'rxjs';
import { UavProfileApi } from '../api/uav-profile.api';

@Injectable({ providedIn: 'root' })
export class ProfilePhotoCache {
  private readonly api = inject(UavProfileApi);
  private readonly pending = new Map<string, Observable<string>>();
  private readonly urls = new Map<string, string>();

  url(id: number, version: number): Observable<string> {
    const key = `${id}:${version}`;
    const cached = this.pending.get(key);
    if (cached) {
      return cached;
    }

    const request = this.api.getPhoto(id).pipe(
      map(blob => {
        const url = URL.createObjectURL(blob);
        this.urls.set(key, url);
        return url;
      }),
      shareReplay({ bufferSize: 1, refCount: false })
    );
    this.pending.set(key, request);
    return request;
  }

  forget(id: number): void {
    for (const key of [...this.pending.keys()]) {
      if (key.startsWith(`${id}:`)) {
        const url = this.urls.get(key);
        if (url) {
          URL.revokeObjectURL(url);
        }
        this.urls.delete(key);
        this.pending.delete(key);
      }
    }
  }
}
