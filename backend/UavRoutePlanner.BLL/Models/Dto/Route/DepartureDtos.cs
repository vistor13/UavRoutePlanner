namespace UavRoutePlanner.BLL.Models.Dto.Route;

public record DepartureSearchDto(DateTime? FromUtc, DateTime? ToUtc, int? Seed);

public record DepartureHourDto(
    DateTime TimeUtc,
    double WindSpeedMs,
    double WindDirectionFromDeg,
    int? VisitedObjectsCount,
    double? TotalTimeS,
    string? ErrorCode,
    bool IsBest,
    RouteResultDto? Route);

public record NoWindBaselineDto(int VisitedObjectsCount, double TotalTimeS);

public record DepartureRecommendationDto(
    DateTime BestDepartureTimeUtc,
    List<DepartureHourDto> Hours,
    NoWindBaselineDto? NoWindBaseline,
    int Seed);

public record SaveDeparturePlanDto(string? Name, DateTime DepartureTimeUtc, int Seed);
