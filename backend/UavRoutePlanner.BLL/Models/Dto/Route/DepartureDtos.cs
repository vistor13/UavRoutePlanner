namespace UavRoutePlanner.BLL.Models.Dto.Route;

public record DepartureSearchDto(int? Hours, int? Seed);


public record DepartureHourDto(
    DateTime TimeUtc,
    double WindSpeedMs,
    double WindDirectionFromDeg,
    int? VisitedObjectsCount,
    double? TotalTimeS,
    string? ErrorCode,
    bool IsBest);

public record NoWindBaselineDto(int VisitedObjectsCount, double TotalTimeS);

public record DepartureRecommendationDto(
    DateTime BestDepartureTimeUtc,
    List<DepartureHourDto> Hours,
    NoWindBaselineDto? NoWindBaseline,
    RouteResultDto BestRoute);
