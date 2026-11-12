using ErrorOr;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Interfaces;

public interface ITimeMatrixBuilder
{
   
    ErrorOr<TimeMatrix> Build(IReadOnlyList<GeoPoint> points, double airspeedMs, Wind wind);
}
