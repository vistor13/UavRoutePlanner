using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Entities;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class WindResolver(IHourlyWindProvider windProvider) : IWindResolver
{
    public async Task<ErrorOr<Wind>> Resolve(
        MissionEntity mission, RouteRequestDto request, DateTime departureHourUtc, CancellationToken ct = default)
    {
        if (request.Mode == WindMode.NoWind)
            return new Wind(0, 0);

        if (request.ManualWind is { } manual)
        {
            var valid = double.IsFinite(manual.SpeedMs) && manual.SpeedMs >= 0
                        && double.IsFinite(manual.DirectionFromDeg) && manual.DirectionFromDeg is >= 0 and <= 360;

            return valid
                ? new Wind(manual.SpeedMs, manual.DirectionFromDeg % 360.0)
                : Error.Validation("ManualWindInvalid", Errors.ManualWindInvalid);
        }

        var hours = await windProvider.GetHourlyWind(mission.GetCenter(), mission.Uav.FlightAltitudeM, ct);
        if (hours.IsError)
            return hours.Errors;

        var hour = hours.Value.FirstOrDefault(h => h.TimeUtc == departureHourUtc);

        return hour is null
            ? Error.Validation("DepartureOutsideForecast", Errors.DepartureOutsideForecast)
            : hour.Wind;
    }
}
