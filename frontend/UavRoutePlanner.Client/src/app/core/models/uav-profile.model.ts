export interface UavProfile {
  id: number;
  name: string;
  airspeedMs: number;
  maxFlightTimeMin: number;
  reservePercent: number;
  flightAltitudeM: number;
}

export type UavProfileInput = Omit<UavProfile, 'id'>;
