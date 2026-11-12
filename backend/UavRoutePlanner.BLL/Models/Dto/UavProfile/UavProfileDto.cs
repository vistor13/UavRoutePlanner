namespace UavRoutePlanner.BLL.Models.Dto.UavProfile;

public record UavProfileDto(
    long Id,
    string Name,
    double AirspeedMs,
    double MaxFlightTimeMin,
    double ReservePercent,
    double FlightAltitudeM);
