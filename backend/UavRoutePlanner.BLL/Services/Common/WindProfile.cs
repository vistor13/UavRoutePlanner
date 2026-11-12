using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Services.Common;
public static class WindProfile
{

    public static Wind AtAltitude(IReadOnlyList<(double HeightM, Wind Wind)> levels, double altitudeM)
    {
        if (levels.Count == 0)
            throw new ArgumentException("Потрібен щонайменше один рівень.", nameof(levels));

        var ordered = levels.OrderBy(l => l.HeightM).ToList();

        if (altitudeM <= ordered[0].HeightM) return ordered[0].Wind;
        if (altitudeM >= ordered[^1].HeightM) return ordered[^1].Wind;

        var upperIndex = ordered.FindIndex(l => l.HeightM >= altitudeM);
        var lower = ordered[upperIndex - 1];
        var upper = ordered[upperIndex];

        var k = (altitudeM - lower.HeightM) / (upper.HeightM - lower.HeightM);

        var (u1, v1) = ToComponents(lower.Wind);
        var (u2, v2) = ToComponents(upper.Wind);

        return FromComponents(u1 + (u2 - u1) * k, v1 + (v2 - v1) * k);
    }

    
    private static (double U, double V) ToComponents(Wind wind)
    {
        var rad = GeoMath.ToRadians(wind.DirectionFromDeg);
        return (-wind.SpeedMs * Math.Sin(rad), -wind.SpeedMs * Math.Cos(rad));
    }

    private static Wind FromComponents(double u, double v)
    {
        var speed = Math.Sqrt(u * u + v * v);
        if (speed < 1e-9) return new Wind(0, 0);

        var direction = GeoMath.Normalize360(GeoMath.ToDegrees(Math.Atan2(-u, -v)));

        
        return new Wind(speed, direction >= 360.0 ? 0.0 : direction);
    }
}
