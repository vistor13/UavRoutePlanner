using ErrorOr;
using Microsoft.Extensions.Options;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Geo;
using UavRoutePlanner.BLL.Models.Options;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;


public class HourlyWindProvider(
    IWeatherSource source,
    IWeatherForecastRepository forecastRepository,
    IOptions<OpenMeteoOptions> options,
    TimeProvider timeProvider) : IHourlyWindProvider
{
    private const double MaxAltitudeM = 180;

    public const int CoordinateDecimals = 2;

    private const int MinCachedHours = 24;

    private readonly OpenMeteoOptions _options = options.Value;

    public async Task<ErrorOr<List<HourlyWind>>> GetHourlyWind(
        GeoPoint point, double altitudeM, CancellationToken ct = default)
    {
        if (!double.IsFinite(point.Latitude) || !double.IsFinite(point.Longitude)
            || point.Latitude is < -90 or > 90 || point.Longitude is < -180 or > 180)
            return Error.Validation("CoordinatesOutOfRange", Errors.CoordinatesOutOfRange);

        if (!double.IsFinite(altitudeM) || altitudeM is <= 0 or > MaxAltitudeM)
            return Error.Validation("AltitudeOutOfRange", Errors.AltitudeOutOfRange);

        var lat = Math.Round(point.Latitude, CoordinateDecimals);
        var lon = Math.Round(point.Longitude, CoordinateDecimals);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var fromUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
        var toUtc = fromUtc.AddDays(_options.ForecastDays);

        var byLevel = await LoadCache(lat, lon, fromUtc, toUtc, ct);

        if (!IsFresh(byLevel, now))
        {
            var fetched = await source.Fetch(lat, lon, fromUtc, ct);

            if (fetched is null)
                return Error.Failure("WeatherServiceUnavailable", Errors.WeatherServiceUnavailable);

            var rows = fetched.Select(s => new WeatherForecastEntity
            {
                Latitude = lat,
                Longitude = lon,
                ForecastTimeUtc = s.TimeUtc,
                HeightM = s.HeightM,
                WindSpeedMs = s.SpeedMs,
                WindDirectionFromDeg = s.DirectionFromDeg,
                FetchedAtUtc = now
            }).ToList();

            await forecastRepository.ReplaceFrom(lat, lon, fromUtc, rows, ct);

            byLevel = GroupByLevel(rows);
        }

        var hours = BuildHourly(byLevel, altitudeM);

        return hours.Count == 0
            ? Error.Failure("WeatherServiceUnavailable", Errors.WeatherServiceUnavailable)
            : hours;
    }

    private async Task<Dictionary<int, List<WeatherForecastEntity>>> LoadCache(
        double lat, double lon, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var result = new Dictionary<int, List<WeatherForecastEntity>>();
        foreach (var level in source.LevelsM)
            result[level] = await forecastRepository.GetRangeAsync(lat, lon, level, fromUtc, toUtc, ct);

        return result;
    }

    private Dictionary<int, List<WeatherForecastEntity>> GroupByLevel(List<WeatherForecastEntity> rows) =>
        source.LevelsM.ToDictionary(l => l, l => rows.Where(r => r.HeightM == l).OrderBy(r => r.ForecastTimeUtc).ToList());

    private bool IsFresh(Dictionary<int, List<WeatherForecastEntity>> byLevel, DateTime now) =>
        byLevel.Values.All(rows =>
            rows.Count >= MinCachedHours
            && rows.All(r => now - r.FetchedAtUtc < TimeSpan.FromMinutes(_options.CacheMinutes)));

    private List<HourlyWind> BuildHourly(Dictionary<int, List<WeatherForecastEntity>> byLevel, double altitudeM)
    {
        var lookup = byLevel.ToDictionary(p => p.Key, p => p.Value.ToDictionary(r => r.ForecastTimeUtc));

        var commonTimes = lookup.Values
            .Select(d => d.Keys.AsEnumerable())
            .Aggregate((a, b) => a.Intersect(b))
            .OrderBy(t => t);

        return commonTimes
            .Select(time => new HourlyWind(
                time,
                WindProfile.AtAltitude(
                    source.LevelsM.Select(l => ((double)l, new Wind(
                        lookup[l][time].WindSpeedMs,
                        lookup[l][time].WindDirectionFromDeg))).ToList(),
                    altitudeM)))
            .ToList();
    }
}
