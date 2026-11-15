import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'The server is unavailable. Make sure the API is running.';
    if (error.status === 401) return 'You need to sign in.';
    const title = error.error?.title;
    if (typeof title === 'string' && title.length > 0) return title;
    return `Error ${error.status}.`;
  }
  return 'An unknown error occurred.';
}
