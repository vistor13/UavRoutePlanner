namespace UavRoutePlanner.BLL.Models.Weather;

public sealed record WindSample(DateTime TimeUtc, int HeightM, double SpeedMs, double DirectionFromDeg);
