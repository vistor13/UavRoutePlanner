using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.DAL.Entities;
using DalWindMode = UavRoutePlanner.DAL.Entities.Enums.WindMode;

namespace UavRoutePlanner.BLL.Services.Common;

public static class RouteMapping
{
    public static RouteResultDto ToDto(this RouteResultEntity entity, MissionEntity mission)
    {
      
        var byCoordinates = mission.Objects
            .GroupBy(o => (o.Latitude, o.Longitude))
            .ToDictionary(g => g.Key, g => g.First());

        var legs = entity.Legs
            .OrderBy(l => l.Order)
            .Select(l => new RouteLegDto(
                l.Order,
                byCoordinates.TryGetValue((l.Latitude, l.Longitude), out var obj) ? obj.Number : null,
                l.PointName, l.Latitude, l.Longitude,
                l.DistanceM, l.TrackDeg, l.HeadingDeg, l.GroundSpeedMs, l.TimeS))
            .ToList();

        var visited = legs.Where(l => l.ObjectNumber is not null).Select(l => l.ObjectNumber!.Value).ToHashSet();

        var skipped = mission.Objects
            .Where(o => !visited.Contains(o.Number))
            .OrderBy(o => o.Number)
            .Select(o => new SkippedObjectDto(o.Number, o.Name, o.Latitude, o.Longitude))
            .ToList();

        return new RouteResultDto(
            entity.Id,
            entity.MissionId,
            entity.CreatedAt,
            entity.WindMode.ToModel(),
            entity.DepartureTimeUtc,
            entity.WindSpeedMs,
            entity.WindDirectionFromDeg,
            entity.VisitedObjectsCount,
            mission.Objects.Count,
            entity.TotalTimeS,
            mission.GetTimeLimitS(),
            legs,
            skipped);
    }

  
    public static WindMode ToModel(this DalWindMode mode) => mode switch
    {
        DalWindMode.WithWind => WindMode.WithWind,
        DalWindMode.NoWind => WindMode.NoWind,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public static DalWindMode ToEntity(this WindMode mode) => mode switch
    {
        WindMode.WithWind => DalWindMode.WithWind,
        WindMode.NoWind => DalWindMode.NoWind,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    
    public static RouteResultEntity ToEntity(
        this RoutePlan plan, MissionEntity mission, WindMode mode, DateTime departureUtc, Wind wind)
    {
        var objects = mission.Objects.OrderBy(o => o.Number).ToList();
        var finishNode = objects.Count + 1;

        return new RouteResultEntity
        {
            MissionId = mission.Id,
            WindMode = mode.ToEntity(),
            DepartureTimeUtc = departureUtc,
            WindSpeedMs = wind.SpeedMs,
            WindDirectionFromDeg = wind.DirectionFromDeg,
            VisitedObjectsCount = plan.VisitedObjects.Count,
            TotalTimeS = plan.TotalTimeS,
            Legs = plan.Legs
                .Select((leg, index) =>
                {
                    var (name, latitude, longitude) = DescribeNode(leg.ToNode, finishNode, mission, objects);

                    return new RouteLegEntity
                    {
                        Order = index + 1,
                        PointName = name,
                        Latitude = latitude,
                        Longitude = longitude,
                        DistanceM = leg.Leg.DistanceM,
                        TrackDeg = leg.Leg.TrackDeg,
                        HeadingDeg = leg.Leg.HeadingDeg,
                        GroundSpeedMs = leg.Leg.GroundSpeedMs,
                        TimeS = leg.Leg.TimeS
                    };
                })
                .ToList()
        };
    }

    private static (string Name, double Latitude, double Longitude) DescribeNode(
        int node, int finishNode, MissionEntity mission, List<SurveyObjectEntity> objects)
    {
        if (node == 0) return ("Старт", mission.Start.Latitude, mission.Start.Longitude);
        if (node == finishNode) return ("Фініш", mission.Finish.Latitude, mission.Finish.Longitude);

        var obj = objects[node - 1];
        return (obj.Name, obj.Latitude, obj.Longitude);
    }
}
