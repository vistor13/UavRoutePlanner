export interface HourlyWind {
  timeUtc: string;
  speedMs: number;
  directionFromDeg: number;
}

export interface WeatherForecast {
  latitude: number;
  longitude: number;
  altitudeM: number;
  hours: HourlyWind[];
}
