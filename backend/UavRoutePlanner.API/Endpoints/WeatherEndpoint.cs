 using System.Security.Claims;
using UavRoutePlanner.Api.Extensions;
using UavRoutePlanner.BLL.Interfaces;

namespace UavRoutePlanner.Api.Endpoints;

public static class WeatherEndpoint
{
    public static void MapWeatherEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api")
            .WithTags("Weather")
            .RequireAuthorization();

       
        group.MapGet("missions/{id:long}/weather", GetForMission);

     
        group.MapGet("weather", GetForPoint);
    }

    private static async Task<IResult> GetForMission(
        long id, ClaimsPrincipal user, IWeatherForecastService service, CancellationToken ct) =>
        (await service.GetForMission(user.GetUserId(), id, ct)).ToResult();

    private static async Task<IResult> GetForPoint(
        double latitude, double longitude, double altitudeM, IWeatherForecastService service, CancellationToken ct) =>
        (await service.GetForecast(latitude, longitude, altitudeM, ct)).ToResult();
}
