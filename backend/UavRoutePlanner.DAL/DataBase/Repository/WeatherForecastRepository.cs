using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class WeatherForecastRepository(ApplicationDbContext context)
    : RepositoriesBase<WeatherForecastEntity>(context), IWeatherForecastRepository
{
    public async Task<List<WeatherForecastEntity>> GetRangeAsync(
        double latitude, double longitude, int heightM, DateTime fromUtc, DateTime toUtc,
        CancellationToken ct = default) =>
        await Context.WeatherForecasts.AsNoTracking()
            .Where(w => w.Latitude == latitude
                        && w.Longitude == longitude
                        && w.HeightM == heightM
                        && w.ForecastTimeUtc >= fromUtc
                        && w.ForecastTimeUtc <= toUtc)
            .OrderBy(w => w.ForecastTimeUtc)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<WeatherForecastEntity> forecasts, CancellationToken ct = default)
    {
        await Context.WeatherForecasts.AddRangeAsync(forecasts, ct);
        await Context.SaveChangesAsync(ct);
    }

    public async Task ReplaceFrom(
        double latitude, double longitude, DateTime fromUtc, IEnumerable<WeatherForecastEntity> forecasts,
        CancellationToken ct = default)
    {
        try
        {
            await using var transaction = await Context.Database.BeginTransactionAsync(ct);

            await Context.WeatherForecasts
                .Where(w => w.Latitude == latitude && w.Longitude == longitude && w.ForecastTimeUtc >= fromUtc)
                .ExecuteDeleteAsync(ct);

            await Context.WeatherForecasts.AddRangeAsync(forecasts, ct);
            await Context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Паралельний запит уже зберіг цей самий прогноз (порушення унікального індексу).
            // Деталі збереження – справа цього рівня, тому BLL про них не знає.
            Context.ChangeTracker.Clear();
        }
    }
}
