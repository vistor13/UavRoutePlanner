using UavRoutePlanner.BLL.Models.Weather;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IWeatherSource
{

    Task<List<WindSample>?> Fetch(double latitude, double longitude, DateTime fromUtc, CancellationToken ct = default);


    IReadOnlyList<int> LevelsM { get; }
}
