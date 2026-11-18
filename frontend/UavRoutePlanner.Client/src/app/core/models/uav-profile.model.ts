export interface UavProfile {
  id: number;
  name: string;
  airspeedMs: number;
  maxFlightTimeMin: number;
  reservePercent: number;
  flightAltitudeM: number;
  photoVersion: number | null;
}

export type UavProfileInput = Omit<UavProfile, 'id' | 'photoVersion'>;

export const UAV_LIMITS = {
  nameMaxLength: 100,
  airspeedMaxMs: 100,
  flightTimeMaxMin: 720,
  reserveMaxPercent: 99,
  altitudeMaxM: 180
} as const;
