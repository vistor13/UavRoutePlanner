using ErrorOr;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Interfaces;


public interface IHourlyWindProvider
{
    Task<ErrorOr<List<HourlyWind>>> GetHourlyWind(GeoPoint point, double altitudeM, CancellationToken ct = default);
}
