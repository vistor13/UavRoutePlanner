using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Weather;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class WeatherForecastService(
    IHourlyWindProvider windProvider,
    IMissionRepository missionRepository) : IWeatherForecastService
{
    public async Task<ErrorOr<WeatherForecastDto>> GetForecast(
        double latitude, double longitude, double altitudeM, CancellationToken ct = default)
    {
        var hours = await windProvider.GetHourlyWind(new GeoPoint(latitude, longitude), altitudeM, ct);

        return hours.IsError ? hours.Errors : ToDto(latitude, longitude, altitudeM, hours.Value);
    }

    public async Task<ErrorOr<WeatherForecastDto>> GetForMission(
        long userId, long missionId, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var center = mission.GetCenter();

        return await GetForecast(center.Latitude, center.Longitude, mission.Uav.FlightAltitudeM, ct);
    }

    private static WeatherForecastDto ToDto(
        double latitude, double longitude, double altitudeM, List<HourlyWind> hours) => new(
        Math.Round(latitude, HourlyWindProvider.CoordinateDecimals),
        Math.Round(longitude, HourlyWindProvider.CoordinateDecimals),
        altitudeM,
        hours.Select(h => new HourlyWindDto(h.TimeUtc, h.Wind.SpeedMs, h.Wind.DirectionFromDeg)).ToList());
}
