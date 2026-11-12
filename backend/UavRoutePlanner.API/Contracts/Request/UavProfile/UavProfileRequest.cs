using UavRoutePlanner.BLL.Models.Dto.UavProfile;

namespace UavRoutePlanner.Api.Contracts.Request.UavProfile;

public record UavProfileRequest(
    string Name,
    double AirspeedMs,
    double MaxFlightTimeMin,
    double ReservePercent,
    double FlightAltitudeM)
{
    public UavProfileInputDto ToDto() => new(Name, AirspeedMs, MaxFlightTimeMin, ReservePercent, FlightAltitudeM);
}
