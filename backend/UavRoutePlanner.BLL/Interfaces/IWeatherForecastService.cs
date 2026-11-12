using ErrorOr;
using UavRoutePlanner.BLL.Models.Dto.Weather;

namespace UavRoutePlanner.BLL.Interfaces;

public interface IWeatherForecastService
{

    Task<ErrorOr<WeatherForecastDto>> GetForecast(
        double latitude, double longitude, double altitudeM, CancellationToken ct = default);

    
    Task<ErrorOr<WeatherForecastDto>> GetForMission(long userId, long missionId, CancellationToken ct = default);
}
