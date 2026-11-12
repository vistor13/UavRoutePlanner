using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;

namespace UavRoutePlanner.BLL.Services;


public class WindTriangleCalculator : IWindTriangleCalculator
{
    public ErrorOr<LegResult> CalculateLeg(GeoPoint from, GeoPoint to, double airspeedMs, Wind wind)
    {
        if (airspeedMs <= 0)
            return Error.Validation("AirspeedNotPositive", Messages.Messages.Error.AirspeedNotPositive);

        if (wind.SpeedMs < 0)
            return Error.Validation("WindSpeedNegative", Messages.Messages.Error.WindSpeedNegative);

        if (wind.SpeedMs >= airspeedMs)
            return Error.Validation("WindExceedsAirspeed", Messages.Messages.Error.WindExceedsAirspeed);

        var distance = GeoMath.DistanceM(from, to);
        if (distance <= 0)
            return Error.Validation("PointsCoincide", Messages.Messages.Error.PointsCoincide);

        var track = GeoMath.InitialBearingDeg(from, to);

        var windAngle = GeoMath.ToRadians(GeoMath.Normalize180(wind.DirectionFromDeg - track));

        var correction = Math.Asin(wind.SpeedMs / airspeedMs * Math.Sin(windAngle));

        var heading = GeoMath.Normalize360(track + GeoMath.ToDegrees(correction));

        var groundSpeed = airspeedMs * Math.Cos(correction) - wind.SpeedMs * Math.Cos(windAngle);

        var time = distance / groundSpeed;

        return new LegResult(
            distance,
            track,
            GeoMath.ToDegrees(correction),
            heading,
            groundSpeed,
            time);
    }
}
