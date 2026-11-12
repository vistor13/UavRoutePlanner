using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Services.Common;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class MissionValidator : IMissionValidator
{
    private const int MaxNameLength = 200;

    private const int MaxObjectsCount = 100;

    private const int MaxAntsCount = 500;
    private const int MaxIterationsCount = 5000;

    private const long MaxComputationBudget = 500_000_000;
    private const double CoincideThresholdM = 1.0;

    public Error? Validate(MissionInputDto input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > MaxNameLength)
            return Error.Validation("MissionName", Errors.MissionName);

        if (input.Start is null || input.Finish is null)
            return Error.Validation("MissionPointsMissing", Errors.MissionPointsMissing);

        var objects = input.Objects ?? [];
        if (objects.Count is 0 or > MaxObjectsCount)
            return Error.Validation("MissionObjectsCount", Errors.MissionObjectsCount);

        if (!IsValidPoint(input.Start.Latitude, input.Start.Longitude)
            || !IsValidPoint(input.Finish.Latitude, input.Finish.Longitude)
            || objects.Any(o => !IsValidPoint(o.Latitude, o.Longitude)))
            return Error.Validation("CoordinatesOutOfRange", Errors.CoordinatesOutOfRange);

        if (objects.Any(o => o.Name is { Length: > MaxNameLength }))
            return Error.Validation("ObjectNameTooLong", Errors.ObjectNameTooLong);

        if (HasCoincidingPoints(input.Start, input.Finish, objects))
            return Error.Validation("ObjectsCoincide", Errors.ObjectsCoincide);

        if (input.Algorithm is { } a && !IsValidAlgorithm(a))
            return Error.Validation("AlgorithmParameters", Errors.AlgorithmParameters);

        var effective = input.Algorithm ?? AlgorithmSettingsDto.Default;
        var size = (long)objects.Count + 2;
        if ((long)effective.AntsCount * effective.IterationsCount * size * size > MaxComputationBudget)
            return Error.Validation("AlgorithmTooHeavy", Errors.AlgorithmTooHeavy);

        return null;
    }

    private static bool IsValidPoint(double latitude, double longitude) =>
        double.IsFinite(latitude) && double.IsFinite(longitude)
        && latitude is >= -90 and <= 90
        && longitude is >= -180 and <= 180;


    private static bool HasCoincidingPoints(
        GeoPointDto start, GeoPointDto finish, List<SurveyObjectInputDto> objects)
    {
        var points = objects.Select(o => new GeoPoint(o.Latitude, o.Longitude)).ToList();
        var startPoint = new GeoPoint(start.Latitude, start.Longitude);
        var finishPoint = new GeoPoint(finish.Latitude, finish.Longitude);

        for (var i = 0; i < points.Count; i++)
        {
            if (GeoMath.DistanceM(points[i], startPoint) < CoincideThresholdM
                || GeoMath.DistanceM(points[i], finishPoint) < CoincideThresholdM)
                return true;

            for (var j = i + 1; j < points.Count; j++)
            {
                if (GeoMath.DistanceM(points[i], points[j]) < CoincideThresholdM)
                    return true;
            }
        }

        return false;
    }

    private static bool IsValidAlgorithm(AlgorithmSettingsDto a) =>
        double.IsFinite(a.Alpha) && a.Alpha >= 0
        && double.IsFinite(a.Beta) && a.Beta >= 0
        && double.IsFinite(a.EvaporationRate) && a.EvaporationRate is > 0 and < 1
        && double.IsFinite(a.PheromoneDeposit) && a.PheromoneDeposit > 0
        && a.AntsCount is >= 1 and <= MaxAntsCount
        && a.IterationsCount is >= 1 and <= MaxIterationsCount;
}
