using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Models.Flight;

public sealed record AntColonySettings(
    double Alpha,
    double Beta,
    double EvaporationRate,
    double PheromoneDeposit,
    int AntsCount,
    int IterationsCount,
    double InitialPheromone = 1.0);


public sealed record RoutePlanRequest(
    GeoPoint Start,
    GeoPoint Finish,
    IReadOnlyList<GeoPoint> Objects,
    double AirspeedMs,
    Wind Wind,
    double MaxTimeS,
    AntColonySettings Settings,
    int? Seed = null);


public sealed record PlannedLeg(int FromNode, int ToNode, LegResult Leg);


public sealed record RoutePlan(
    IReadOnlyList<int> VisitedObjects,
    IReadOnlyList<PlannedLeg> Legs,
    double TotalTimeS);


public sealed record TimeMatrix(double[,] Times, LegResult?[,] Legs);
