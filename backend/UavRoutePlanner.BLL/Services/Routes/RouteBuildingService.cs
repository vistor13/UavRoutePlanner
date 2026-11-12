using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class RouteBuildingService(
    IMissionRepository missionRepository,
    IRouteResultRepository routeResultRepository,
    IRoutePlanner planner,
    IWindResolver windResolver,
    TimeProvider timeProvider) : IRouteBuildingService
{
    public async Task<ErrorOr<RouteResultDto>> Build(
        long userId, long missionId, RouteRequestDto request, CancellationToken ct = default)
    {
        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var departure = GetDepartureHour(request.DepartureTimeUtc);

        var wind = await windResolver.Resolve(mission, request, departure, ct);
        if (wind.IsError)
            return wind.Errors;

        var plan = planner.Plan(mission.ToPlanRequest(wind.Value, request.Seed), ct);
        if (plan.IsError)
            return plan.Errors;

        var entity = plan.Value.ToEntity(mission, request.Mode, departure, wind.Value);
        var created = await routeResultRepository.Create(entity, ct);

        return created.ToDto(mission);
    }

    private DateTime GetDepartureHour(DateTime? requested)
    {
        var time = requested switch
        {
            null => timeProvider.GetUtcNow().UtcDateTime,
            { Kind: DateTimeKind.Local } local => local.ToUniversalTime(),
            { } other => DateTime.SpecifyKind(other, DateTimeKind.Utc)
        };

        return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Utc);
    }
}
