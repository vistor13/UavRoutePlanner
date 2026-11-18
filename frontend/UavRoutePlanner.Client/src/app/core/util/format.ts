export function formatDuration(seconds: number): string {
  const total = Math.round(seconds);
  const minutes = Math.floor(total / 60);
  const rest = total % 60;
  return minutes > 0 ? `${minutes} min ${rest} s` : `${rest} s`;
}

export function formatDistance(meters: number): string {
  return meters >= 1000 ? `${(meters / 1000).toFixed(2)} km` : `${Math.round(meters)} m`;
}

export function formatUtcHour(iso: string): string {
  const date = new Date(iso);
  const pad = (value: number) => value.toString().padStart(2, '0');
  return `${pad(date.getUTCDate())}.${pad(date.getUTCMonth() + 1)} ${pad(date.getUTCHours())}:00 UTC`;
}

export function toUtcHourIso(localValue: string): string | null {
  if (!localValue) return null;
  const date = new Date(localValue);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

const pad = (value: number): string => value.toString().padStart(2, '0');

export function formatLocalHour(iso: string): string {
  const date = new Date(iso);
  return `${pad(date.getDate())}.${pad(date.getMonth() + 1)} ${pad(date.getHours())}:00`;
}

export function formatLocalDateTime(iso: string): string {
  const date = new Date(iso);
  return `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()} ${pad(date.getHours())}:00`;
}

export function zoneLabel(iso?: string): string {
  const date = iso ? new Date(iso) : new Date();
  const offset = -date.getTimezoneOffset();
  const sign = offset >= 0 ? '+' : '-';
  const absolute = Math.abs(offset);
  const hours = Math.floor(absolute / 60);
  const minutes = absolute % 60;
  return `UTC${sign}${hours}${minutes ? ':' + pad(minutes) : ''}`;
}

export function toLocalInput(date: Date): string {
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:00`;
}

export function floorToLocalHour(date: Date): Date {
  const result = new Date(date);
  result.setMinutes(0, 0, 0);
  return result;
}
