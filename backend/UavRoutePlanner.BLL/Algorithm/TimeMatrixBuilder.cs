using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;

namespace UavRoutePlanner.BLL.Algorithm;
public class TimeMatrixBuilder(IWindTriangleCalculator calculator) : ITimeMatrixBuilder
{
    
    private const double CoincideThresholdM = 1.0;

    public ErrorOr<TimeMatrix> Build(IReadOnlyList<GeoPoint> points, double airspeedMs, Wind wind)
    {
        var size = points.Count;
        var times = new double[size, size];
        var legs = new LegResult?[size, size];

        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                if (i == j) continue;

                if (GeoMath.DistanceM(points[i], points[j]) < CoincideThresholdM)
                    continue;

                var result = calculator.CalculateLeg(points[i], points[j], airspeedMs, wind);
                if (result.IsError)
                    return result.Errors;

                times[i, j] = result.Value.TimeS;
                legs[i, j] = result.Value;
            }
        }

        return new TimeMatrix(times, legs);
    }
}
