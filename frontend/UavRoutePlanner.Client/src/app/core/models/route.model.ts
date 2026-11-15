export type WindMode = 'WithWind' | 'NoWind';

export type ExportFormat = 'csv' | 'geojson' | 'waypoints';

export interface RouteLeg {
  order: number;
  objectNumber: number | null;
  pointName: string;
  latitude: number;
  longitude: number;
  distanceM: number;
  trackDeg: number;
  headingDeg: number;
  groundSpeedMs: number;
  timeS: number;
}

export interface SkippedObject {
  number: number;
  name: string;
  latitude: number;
  longitude: number;
}

export interface RouteResult {
  id: number;
  missionId: number;
  createdAt: string;
  windMode: WindMode;
  departureTimeUtc: string;
  windSpeedMs: number;
  windDirectionFromDeg: number;
  visitedObjectsCount: number;
  totalObjectsCount: number;
  totalTimeS: number;
  timeLimitS: number;
  legs: RouteLeg[];
  skippedObjects: SkippedObject[];
}

export interface ManualWind {
  speedMs: number;
  directionFromDeg: number;
}

export interface RouteRequest {
  mode: WindMode;
  departureTimeUtc: string | null;
  manualWind: ManualWind | null;
  seed: number | null;
}

export interface RouteComparison {
  withWind: RouteResult;
  noWind: RouteResult;
  objectsLostDueToWind: number;
  timeDifferenceS: number;
}

export interface DepartureHour {
  timeUtc: string;
  windSpeedMs: number;
  windDirectionFromDeg: number;
  visitedObjectsCount: number | null;
  totalTimeS: number | null;
  errorCode: string | null;
  isBest: boolean;
}

export interface DepartureRecommendation {
  bestDepartureTimeUtc: string;
  hours: DepartureHour[];
  noWindBaseline: { visitedObjectsCount: number; totalTimeS: number } | null;
  bestRoute: RouteResult;
}
