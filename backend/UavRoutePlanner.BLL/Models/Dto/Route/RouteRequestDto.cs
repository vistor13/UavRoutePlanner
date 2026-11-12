using UavRoutePlanner.BLL.Models.Flight;

namespace UavRoutePlanner.BLL.Models.Dto.Route;

public record ManualWindDto(double SpeedMs, double DirectionFromDeg);


public record RouteRequestDto(
    WindMode Mode,
    DateTime? DepartureTimeUtc,
    ManualWindDto? ManualWind,
    int? Seed);
