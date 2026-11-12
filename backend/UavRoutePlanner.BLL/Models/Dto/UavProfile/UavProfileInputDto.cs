namespace UavRoutePlanner.BLL.Models.Dto.UavProfile;

public record UavProfileInputDto(
    string Name,
    double AirspeedMs,
    double MaxFlightTimeMin,
    double ReservePercent,
    double FlightAltitudeM);
