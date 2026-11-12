using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.BLL.Services.Common;

public static class MissionPlanning
{
    public static RoutePlanRequest ToPlanRequest(this MissionEntity mission, Wind wind, int? seed)
    {
        var algorithm = mission.Algorithm;

        return new RoutePlanRequest(
            new GeoPoint(mission.Start.Latitude, mission.Start.Longitude),
            new GeoPoint(mission.Finish.Latitude, mission.Finish.Longitude),
            mission.Objects.OrderBy(o => o.Number).Select(o => new GeoPoint(o.Latitude, o.Longitude)).ToList(),
            mission.Uav.AirspeedMs,
            wind,
            mission.GetTimeLimitS(),
            new AntColonySettings(
                algorithm.Alpha,
                algorithm.Beta,
                algorithm.EvaporationRate,
                algorithm.PheromoneDeposit,
                algorithm.AntsCount,
                algorithm.IterationsCount),
            seed);
    }
}
