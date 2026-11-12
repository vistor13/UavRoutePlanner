using UavRoutePlanner.BLL.Models.Flight;

namespace UavRoutePlanner.BLL.Models.Dto.Route;

public record RouteLegDto(
    int Order,
    int? ObjectNumber,
    string PointName,
    double Latitude,
    double Longitude,
    double DistanceM,
    double TrackDeg,
    double HeadingDeg,
    double GroundSpeedMs,
    double TimeS);

public record SkippedObjectDto(int Number, string Name, double Latitude, double Longitude);

public record RouteResultDto(
    long Id,
    long MissionId,
    DateTime CreatedAt,
    WindMode WindMode,
    DateTime DepartureTimeUtc,
    double WindSpeedMs,
    double WindDirectionFromDeg,
    int VisitedObjectsCount,
    int TotalObjectsCount,
    double TotalTimeS,
    double TimeLimitS,
    List<RouteLegDto> Legs,
    List<SkippedObjectDto> SkippedObjects);

public record RouteComparisonDto(
    RouteResultDto WithWind,
    RouteResultDto NoWind,
    int ObjectsLostDueToWind,
    double TimeDifferenceS);
