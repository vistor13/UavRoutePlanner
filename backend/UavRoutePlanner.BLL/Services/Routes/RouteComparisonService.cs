using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Flight;

namespace UavRoutePlanner.BLL.Services;

public class RouteComparisonService(IRouteBuildingService routeBuilder) : IRouteComparisonService
{
    public async Task<ErrorOr<RouteComparisonDto>> Compare(
        long userId, long missionId, RouteRequestDto request, CancellationToken ct = default)
    {
        var seed = request.Seed ?? Random.Shared.Next();

        var withWind = await routeBuilder.Build(userId, missionId, request with { Mode = WindMode.WithWind, Seed = seed }, ct);
        if (withWind.IsError)
            return withWind.Errors;

        var noWind = await routeBuilder.Build(userId, missionId, request with { Mode = WindMode.NoWind, Seed = seed }, ct);
        if (noWind.IsError)
            return noWind.Errors;

        return new RouteComparisonDto(
            withWind.Value,
            noWind.Value,
            noWind.Value.VisitedObjectsCount - withWind.Value.VisitedObjectsCount,
            withWind.Value.TotalTimeS - noWind.Value.TotalTimeS);
    }
}
