using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class DepartureTimeService(
    IMissionRepository missionRepository,
    IHourlyWindProvider windProvider,
    IRoutePlanner planner,
    IRouteBuildingService routeBuilder) : IDepartureTimeService
{
    private const int DefaultHours = 24;
    private const int MaxHours = 72;

    public async Task<ErrorOr<DepartureRecommendationDto>> FindBest(
        long userId, long missionId, DepartureSearchDto request, CancellationToken ct = default)
    {
        var hoursCount = request.Hours ?? DefaultHours;
        if (hoursCount is < 1 or > MaxHours)
            return Error.Validation("DepartureHoursOutOfRange", Errors.DepartureHoursOutOfRange);

        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var forecast = await windProvider.GetHourlyWind(mission.GetCenter(), mission.Uav.FlightAltitudeM, ct);
        if (forecast.IsError)
            return forecast.Errors;

        var hours = forecast.Value.Take(hoursCount).ToList();

        
        var seed = request.Seed ?? Random.Shared.Next();

        var outcomes = new ErrorOr<RoutePlan>[hours.Count];
        Parallel.For(0, hours.Count, new ParallelOptions { CancellationToken = ct }, i =>
            outcomes[i] = planner.Plan(mission.ToPlanRequest(hours[i].Wind, seed), ct));

        var best = PickBest(outcomes);
        if (best is null)
            return Error.Validation("NoFeasibleDeparture", Errors.NoFeasibleDeparture);

        var baseline = planner.Plan(mission.ToPlanRequest(new Wind(0, 0), seed), ct);

        
        var saved = await routeBuilder.Build(
            userId, missionId,
            new RouteRequestDto(WindMode.WithWind, hours[best.Value].TimeUtc, null, seed), ct);
        if (saved.IsError)
            return saved.Errors;

        var rows = hours
            .Select((h, i) => new DepartureHourDto(
                h.TimeUtc,
                h.Wind.SpeedMs,
                h.Wind.DirectionFromDeg,
                outcomes[i].IsError ? null : outcomes[i].Value.VisitedObjects.Count,
                outcomes[i].IsError ? null : outcomes[i].Value.TotalTimeS,
                outcomes[i].IsError ? outcomes[i].FirstError.Code : null,
                i == best.Value))
            .ToList();

        return new DepartureRecommendationDto(
            hours[best.Value].TimeUtc,
            rows,
            baseline.IsError ? null : new NoWindBaselineDto(baseline.Value.VisitedObjects.Count, baseline.Value.TotalTimeS),
            saved.Value);
    }

    
    private static int? PickBest(ErrorOr<RoutePlan>[] outcomes)
    {
        int? best = null;

        for (var i = 0; i < outcomes.Length; i++)
        {
            if (outcomes[i].IsError) continue;

            if (best is null || IsBetter(outcomes[i].Value, outcomes[best.Value].Value))
                best = i;
        }

        return best;
    }

    private static bool IsBetter(RoutePlan candidate, RoutePlan current) =>
        candidate.VisitedObjects.Count > current.VisitedObjects.Count
        || (candidate.VisitedObjects.Count == current.VisitedObjects.Count
            && candidate.TotalTimeS < current.TotalTimeS);
}
