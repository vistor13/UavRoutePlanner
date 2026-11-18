using UavRoutePlanner.BLL.Models.Dto.Mission;
using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Entities.CommonEntity;

namespace UavRoutePlanner.BLL.Services.Common;

public static class MissionMapping
{
    public static MissionSummaryDto ToSummaryDto(this MissionEntity entity) => new(
        entity.Id,
        entity.Name,
        entity.CreatedAt,
        entity.UavProfileId,
        entity.Objects.Count,
        entity.Results.Count,
        entity.Results.Count == 0 ? null : entity.Results.Max(r => r.VisitedObjectsCount),
        entity.PlannedDepartureUtc);

    public static MissionDto ToDto(this MissionEntity entity) => new(
        entity.Id,
        entity.Name,
        entity.CreatedAt,
        new GeoPointDto(entity.Start.Latitude, entity.Start.Longitude),
        new GeoPointDto(entity.Finish.Latitude, entity.Finish.Longitude),
        entity.UavProfileId,
        new UavParametersDto(
            entity.Uav.AirspeedMs,
            entity.Uav.MaxFlightTimeMin,
            entity.Uav.ReservePercent,
            entity.Uav.FlightAltitudeM),
        new AlgorithmSettingsDto(
            entity.Algorithm.Alpha,
            entity.Algorithm.Beta,
            entity.Algorithm.EvaporationRate,
            entity.Algorithm.PheromoneDeposit,
            entity.Algorithm.AntsCount,
            entity.Algorithm.IterationsCount),
        entity.Objects
            .OrderBy(o => o.Number)
            .Select(o => new SurveyObjectDto(o.Id, o.Number, o.Name, o.Latitude, o.Longitude))
            .ToList(),
        entity.PlannedDepartureUtc);

    public static MissionEntity ToEntity(
        this MissionInputDto input, long userId, UavProfileEntity profile, GeoPointDto start, GeoPointDto finish)
    {
        var algorithm = input.Algorithm ?? AlgorithmSettingsDto.Default;

        return new MissionEntity
        {
            Name = input.Name.Trim(),
            UserId = userId,
            UavProfileId = profile.Id,
            Start = new GeoPointEntity { Latitude = start.Latitude, Longitude = start.Longitude },
            Finish = new GeoPointEntity { Latitude = finish.Latitude, Longitude = finish.Longitude },
            Uav = new UavParametersEntity
            {
                AirspeedMs = profile.AirspeedMs,
                MaxFlightTimeMin = profile.MaxFlightTimeMin,
                ReservePercent = profile.ReservePercent,
                FlightAltitudeM = profile.FlightAltitudeM
            },
            Algorithm = new AlgorithmSettingsEntity
            {
                Alpha = algorithm.Alpha,
                Beta = algorithm.Beta,
                EvaporationRate = algorithm.EvaporationRate,
                PheromoneDeposit = algorithm.PheromoneDeposit,
                AntsCount = algorithm.AntsCount,
                IterationsCount = algorithm.IterationsCount
            },
            Objects = input.Objects
                .Select((o, i) => new SurveyObjectEntity
                {
                    Number = i + 1,
                    Name = string.IsNullOrWhiteSpace(o.Name) ? $"Об'єкт {i + 1}" : o.Name.Trim(),
                    Latitude = o.Latitude,
                    Longitude = o.Longitude
                })
                .ToList()
        };
    }
}
