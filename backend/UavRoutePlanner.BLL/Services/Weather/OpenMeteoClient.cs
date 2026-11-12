using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Options;
using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Services;

/// <summary>Клієнт Open-Meteo: формує запит і розбирає відповідь. Нічого не знає про кеш і базу.</summary>
public class OpenMeteoClient(HttpClient httpClient, IOptions<OpenMeteoOptions> options) : IWeatherSource
{
    private static readonly int[] Levels = [10, 80, 120, 180];

    private readonly OpenMeteoOptions _options = options.Value;

    public IReadOnlyList<int> LevelsM => Levels;

    public async Task<List<WindSample>?> Fetch(
        double latitude, double longitude, DateTime fromUtc, CancellationToken ct = default)
    {
        var variables = string.Join(',', Levels.SelectMany(l => new[] { $"wind_speed_{l}m", $"wind_direction_{l}m" }));
        var url = string.Create(CultureInfo.InvariantCulture,
            $"v1/forecast?latitude={latitude}&longitude={longitude}&hourly={variables}&wind_speed_unit=ms&timezone=UTC&forecast_days={_options.ForecastDays}");

        try
        {
            using var response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            return ParseResponse(json.RootElement, fromUtc);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException
                                      && !ct.IsCancellationRequested)
        {
            return null;
        }
    }

    /// <summary>Розбирає відповідь Open-Meteo. Години з порожніми (null) значеннями пропускаються.</summary>
    internal static List<WindSample>? ParseResponse(JsonElement root, DateTime fromUtc)
    {
        if (!root.TryGetProperty("hourly", out var hourly)
            || !hourly.TryGetProperty("time", out var times)
            || times.ValueKind != JsonValueKind.Array)
            return null;

        var rows = new List<WindSample>();

        foreach (var level in Levels)
        {
            if (!hourly.TryGetProperty($"wind_speed_{level}m", out var speeds)
                || !hourly.TryGetProperty($"wind_direction_{level}m", out var dirs)
                || speeds.ValueKind != JsonValueKind.Array
                || dirs.ValueKind != JsonValueKind.Array)
                return null;

            var count = Math.Min(times.GetArrayLength(), Math.Min(speeds.GetArrayLength(), dirs.GetArrayLength()));

            for (var i = 0; i < count; i++)
            {
                if (speeds[i].ValueKind != JsonValueKind.Number || dirs[i].ValueKind != JsonValueKind.Number)
                    continue;

                if (!DateTime.TryParseExact(times[i].GetString(), "yyyy-MM-dd'T'HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var time))
                    continue;

                if (time < fromUtc) continue;

                rows.Add(new WindSample(
                    DateTime.SpecifyKind(time, DateTimeKind.Utc), level, speeds[i].GetDouble(), dirs[i].GetDouble()));
            }
        }

        return rows.Count == 0 ? null : rows;
    }
}
