namespace UavRoutePlanner.BLL.Models.Weather;


public sealed record HourlyWind(DateTime TimeUtc, Wind Wind);
