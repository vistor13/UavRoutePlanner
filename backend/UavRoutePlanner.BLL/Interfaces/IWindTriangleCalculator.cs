using ErrorOr;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IWindTriangleCalculator
{
   
    ErrorOr<LegResult> CalculateLeg(GeoPoint from, GeoPoint to, double airspeedMs, Wind wind);
}
