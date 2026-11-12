using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Flight;

namespace UavRoutePlanner.Api.Contracts.Request.Route;

public record ManualWindRequest(double SpeedMs, double DirectionFromDeg)
{
    public ManualWindDto ToDto() => new(SpeedMs, DirectionFromDeg);
}


public record RouteRequest(
    WindMode Mode,
    DateTime? DepartureTimeUtc,
    ManualWindRequest? ManualWind,
    int? Seed)
{
    public RouteRequestDto ToDto() => new(Mode, DepartureTimeUtc, ManualWind?.ToDto(), Seed);
}
