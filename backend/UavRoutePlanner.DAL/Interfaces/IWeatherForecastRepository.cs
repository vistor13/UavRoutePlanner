using UavRoutePlanner.DAL.Entities;

namespace UavRoutePlanner.DAL.Interfaces;

public interface IWeatherForecastRepository : IRepositoriesBase<WeatherForecastEntity>
{
    Task<List<WeatherForecastEntity>> GetRangeAsync(
        double latitude, double longitude, int heightM, DateTime fromUtc, DateTime toUtc,
        CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<WeatherForecastEntity> forecasts, CancellationToken ct = default);

    Task ReplaceFrom(
        double latitude, double longitude, DateTime fromUtc, IEnumerable<WeatherForecastEntity> forecasts,
        CancellationToken ct = default);
}
