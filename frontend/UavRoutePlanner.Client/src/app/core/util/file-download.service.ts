import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class FileDownloadService {
  save(response: HttpResponse<Blob>, fallbackName: string): void {
    const blob = response.body;
    if (!blob) return;

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = this.fileName(response) ?? fallbackName;
    link.click();
    URL.revokeObjectURL(url);
  }

  private fileName(response: HttpResponse<Blob>): string | null {
    const header = response.headers.get('Content-Disposition');
    if (!header) return null;

    const encoded = /filename\*=UTF-8''([^;]+)/i.exec(header);
    if (encoded) return decodeURIComponent(encoded[1]);

    const plain = /filename="?([^";]+)"?/i.exec(header);
    return plain ? plain[1] : null;
  }
}
