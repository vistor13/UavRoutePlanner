import { GeoPoint } from './geo.model';

export interface AlgorithmSettings {
  alpha: number;
  beta: number;
  evaporationRate: number;
  pheromoneDeposit: number;
  antsCount: number;
  iterationsCount: number;
}

export const DEFAULT_ALGORITHM: AlgorithmSettings = {
  alpha: 1,
  beta: 1,
  evaporationRate: 0.1,
  pheromoneDeposit: 0.05,
  antsCount: 30,
  iterationsCount: 150
};

export interface MissionSummary {
  id: number;
  name: string;
  createdAt: string;
  uavProfileId: number | null;
  objectsCount: number;
  routesCount: number;
  bestVisitedObjectsCount: number | null;
}

export interface SurveyObject {
  id: number;
  number: number;
  name: string;
  latitude: number;
  longitude: number;
}

export interface UavParameters {
  airspeedMs: number;
  maxFlightTimeMin: number;
  reservePercent: number;
  flightAltitudeM: number;
}

export interface Mission {
  id: number;
  name: string;
  createdAt: string;
  start: GeoPoint;
  finish: GeoPoint;
  uavProfileId: number | null;
  uav: UavParameters;
  algorithm: AlgorithmSettings;
  objects: SurveyObject[];
}

export interface SurveyObjectInput {
  name: string | null;
  latitude: number;
  longitude: number;
}

export interface MissionInput {
  name: string;
  start: GeoPoint;
  finish: GeoPoint;
  uavProfileId: number;
  objects: SurveyObjectInput[];
  algorithm: AlgorithmSettings | null;
}
