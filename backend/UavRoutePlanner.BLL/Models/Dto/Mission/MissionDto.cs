namespace UavRoutePlanner.BLL.Models.Dto.Mission;

public record SurveyObjectDto(long Id, int Number, string Name, double Latitude, double Longitude);

public record UavParametersDto(
    double AirspeedMs,
    double MaxFlightTimeMin,
    double ReservePercent,
    double FlightAltitudeM);

public record MissionSummaryDto(
    long Id,
    string Name,
    DateTime CreatedAt,
    long? UavProfileId,
    int ObjectsCount,
    int RoutesCount,
    int? BestVisitedObjectsCount,
    DateTime? PlannedDepartureUtc);

public record MissionDto(
    long Id,
    string Name,
    DateTime CreatedAt,
    GeoPointDto Start,
    GeoPointDto Finish,
    long? UavProfileId,
    UavParametersDto Uav,
    AlgorithmSettingsDto Algorithm,
    List<SurveyObjectDto> Objects,
    DateTime? PlannedDepartureUtc);
