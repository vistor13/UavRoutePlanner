namespace UavRoutePlanner.BLL.Models.Dto.Weather;

public record HourlyWindDto(DateTime TimeUtc, double SpeedMs, double DirectionFromDeg);

public record WeatherForecastDto(
    double Latitude,
    double Longitude,
    double AltitudeM,
    List<HourlyWindDto> Hours);
