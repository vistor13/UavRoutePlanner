using System.Globalization;
using System.Text;
using System.Text.Json;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;

namespace UavRoutePlanner.BLL.Services;


public class WaypointsFlightTaskFormatter : IFlightTaskFormatter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private const int CmdWaypoint = 16;
    private const int CmdLand = 21;
    private const int CmdTakeoff = 22;

    private const int FrameGlobal = 0;
    private const int FrameRelativeAlt = 3;

    public ExportFormat Format => ExportFormat.Waypoints;

    public ExportedFileDto Create(MissionDto mission, RouteResultDto route) => new(
        $"route_{route.MissionId}_{route.Id}.waypoints", "text/plain; charset=utf-8",
        Encoding.UTF8.GetBytes(BuildWaypoints(mission, route)));

    
    private static string BuildWaypoints(MissionDto mission, RouteResultDto route)
    {
        var altitude = mission.Uav.FlightAltitudeM;
        var sb = new StringBuilder("QGC WPL 110\n");
        var seq = 0;

        void Row(int current, int frame, int command, double yaw, double lat, double lon, double alt) =>
            sb.Append(string.Join('\t',
                seq++.ToString(Inv), current.ToString(Inv), frame.ToString(Inv), command.ToString(Inv),
                "0", "0", "0", yaw.ToString("F1", Inv),
                lat.ToString("F7", Inv), lon.ToString("F7", Inv), alt.ToString("F2", Inv), "1")).Append('\n');

        var firstHeading = route.Legs.Count > 0 ? route.Legs[0].HeadingDeg : 0;

        Row(1, FrameGlobal, CmdWaypoint, 0, mission.Start.Latitude, mission.Start.Longitude, 0);
        Row(0, FrameRelativeAlt, CmdTakeoff, firstHeading, mission.Start.Latitude, mission.Start.Longitude, altitude);

        for (var i = 0; i < route.Legs.Count - 1; i++)
        {
            var leg = route.Legs[i];
            Row(0, FrameRelativeAlt, CmdWaypoint, route.Legs[i + 1].HeadingDeg, leg.Latitude, leg.Longitude, altitude);
        }

        Row(0, FrameRelativeAlt, CmdLand, 0, mission.Finish.Latitude, mission.Finish.Longitude, 0);

        return sb.ToString();
    }
}
