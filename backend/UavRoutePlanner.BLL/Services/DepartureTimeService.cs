using ErrorOr;
using UavRoutePlanner.BLL.Interfaces;
using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.BLL.Models.Dto.Route;
using UavRoutePlanner.BLL.Models.Flight;
using UavRoutePlanner.BLL.Models.Weather;
using UavRoutePlanner.BLL.Services.Common;
using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Entities.CommonEntity;
using UavRoutePlanner.DAL.Interfaces;
using Errors = UavRoutePlanner.BLL.Messages.Messages.Error;

namespace UavRoutePlanner.BLL.Services;

public class DepartureTimeService(
    IMissionRepository missionRepository,
    IHourlyWindProvider windProvider,
    IRoutePlanner planner,
    TimeProvider timeProvider) : IDepartureTimeService
{
    private const int DefaultHours = 24;
    private const int MaxHours = 72;
    private const int MaxNameLength = 200;

    public async Task<ErrorOr<DepartureRecommendationDto>> FindBest(
        long userId, long missionId, DepartureSearchDto request, CancellationToken ct = default)
    {
        var range = ResolveRange(request);
        if (range.IsError)
            return range.Errors;

        var (from, to) = range.Value;

        var mission = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (mission is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var forecast = await windProvider.GetHourlyWind(mission.GetCenter(), mission.Uav.FlightAltitudeM, ct);
        if (forecast.IsError)
            return forecast.Errors;

        var hours = forecast.Value.Where(h => h.TimeUtc >= from && h.TimeUtc <= to).ToList();
        if (hours.Count == 0)
            return Error.Validation("DepartureOutsideForecast", Errors.DepartureOutsideForecast);

        var seed = request.Seed ?? Random.Shared.Next();

        var outcomes = new ErrorOr<RoutePlan>[hours.Count];
        Parallel.For(0, hours.Count, new ParallelOptions { CancellationToken = ct }, i =>
            outcomes[i] = planner.Plan(mission.ToPlanRequest(hours[i].Wind, seed), ct));

        var best = PickBest(outcomes);
        if (best is null)
            return Error.Validation("NoFeasibleDeparture", Errors.NoFeasibleDeparture);

        var baseline = planner.Plan(mission.ToPlanRequest(new Wind(0, 0), seed), ct);

        var rows = hours
            .Select((h, i) => new DepartureHourDto(
                h.TimeUtc,
                h.Wind.SpeedMs,
                h.Wind.DirectionFromDeg,
                outcomes[i].IsError ? null : outcomes[i].Value.VisitedObjects.Count,
                outcomes[i].IsError ? null : outcomes[i].Value.TotalTimeS,
                outcomes[i].IsError ? outcomes[i].FirstError.Code : null,
                i == best.Value,
                outcomes[i].IsError
                    ? null
                    : outcomes[i].Value.ToEntity(mission, WindMode.WithWind, h.TimeUtc, h.Wind).ToDto(mission)))
            .ToList();

        return new DepartureRecommendationDto(
            hours[best.Value].TimeUtc,
            rows,
            baseline.IsError ? null : new NoWindBaselineDto(baseline.Value.VisitedObjects.Count, baseline.Value.TotalTimeS),
            seed);
    }

    public async Task<ErrorOr<MissionDto>> SavePlan(
        long userId, long missionId, SaveDeparturePlanDto request, CancellationToken ct = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > MaxNameLength)
            return Error.Validation("MissionName", Errors.MissionName);

        var source = await missionRepository.GetOwnedFullAsync(missionId, userId, ct);
        if (source is null)
            return Error.NotFound("MissionNotFound", Errors.MissionNotFound);

        var departure = FloorToHour(request.DepartureTimeUtc);

        var forecast = await windProvider.GetHourlyWind(source.GetCenter(), source.Uav.FlightAltitudeM, ct);
        if (forecast.IsError)
            return forecast.Errors;

        var hour = forecast.Value.FirstOrDefault(h => h.TimeUtc == departure);
        if (hour is null)
            return Error.Validation("DepartureHourNotInForecast", Errors.DepartureHourNotInForecast);

        var plan = planner.Plan(source.ToPlanRequest(hour.Wind, request.Seed), ct);
        if (plan.IsError)
            return plan.Errors;

        var copy = CopyMission(source, name, departure);
        copy.Results.Add(plan.Value.ToEntity(copy, WindMode.WithWind, departure, hour.Wind));

        var created = await missionRepository.Create(copy, ct);

        return created.ToDto();
    }

    private static MissionEntity CopyMission(MissionEntity source, string name, DateTime departure) => new()
    {
        Name = name,
        UserId = source.UserId,
        UavProfileId = source.UavProfileId,
        PlannedDepartureUtc = departure,
        Start = new GeoPointEntity { Latitude = source.Start.Latitude, Longitude = source.Start.Longitude },
        Finish = new GeoPointEntity { Latitude = source.Finish.Latitude, Longitude = source.Finish.Longitude },
        Uav = new UavParametersEntity
        {
            AirspeedMs = source.Uav.AirspeedMs,
            MaxFlightTimeMin = source.Uav.MaxFlightTimeMin,
            ReservePercent = source.Uav.ReservePercent,
            FlightAltitudeM = source.Uav.FlightAltitudeM
        },
        Algorithm = new AlgorithmSettingsEntity
        {
            Alpha = source.Algorithm.Alpha,
            Beta = source.Algorithm.Beta,
            EvaporationRate = source.Algorithm.EvaporationRate,
            PheromoneDeposit = source.Algorithm.PheromoneDeposit,
            AntsCount = source.Algorithm.AntsCount,
            IterationsCount = source.Algorithm.IterationsCount
        },
        Objects = source.Objects
            .OrderBy(o => o.Number)
            .Select(o => new SurveyObjectEntity
            {
                Number = o.Number,
                Name = o.Name,
                Latitude = o.Latitude,
                Longitude = o.Longitude
            })
            .ToList()
    };

    private ErrorOr<(DateTime From, DateTime To)> ResolveRange(DepartureSearchDto request)
    {
        var from = FloorToHour(request.FromUtc ?? timeProvider.GetUtcNow().UtcDateTime);
        var to = request.ToUtc is { } end
            ? FloorToHour(end)
            : from.AddHours(DefaultHours - 1);

        if (to < from)
            return Error.Validation("DepartureRangeInvalid", Errors.DepartureRangeInvalid);

        var count = (to - from).TotalHours + 1;
        if (count > MaxHours)
            return Error.Validation("DepartureHoursOutOfRange", Errors.DepartureHoursOutOfRange);

        return (from, to);
    }

    private static DateTime FloorToHour(DateTime value)
    {
        var utc = value.ToUniversalTime();
        return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
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
