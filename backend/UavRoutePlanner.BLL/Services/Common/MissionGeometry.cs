using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.BLL.Services.Common;

public static class MissionGeometry
{
    
    public static GeoPoint GetCenter(this MissionEntity mission)
    {
        var lats = mission.Objects.Select(o => o.Latitude)
            .Append(mission.Start.Latitude).Append(mission.Finish.Latitude);
        var lons = mission.Objects.Select(o => o.Longitude)
            .Append(mission.Start.Longitude).Append(mission.Finish.Longitude);

        return new GeoPoint(lats.Average(), lons.Average());
    }

    public static double GetTimeLimitS(this MissionEntity mission) =>
        mission.Uav.MaxFlightTimeMin * 60.0 * (1.0 - mission.Uav.ReservePercent / 100.0);
}
