using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class MissionRepository(ApplicationDbContext context)
    : RepositoriesBase<MissionEntity>(context), IMissionRepository
{
    public async Task<List<MissionEntity>> GetByUserIdAsync(long userId, CancellationToken ct = default) =>
        await Context.Missions.AsNoTracking()
            .Include(m => m.Objects)
            .Include(m => m.Results)
            .AsSplitQuery()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task<MissionEntity?> GetFullAsync(long id, CancellationToken ct = default) =>
        await FullQuery().FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<MissionEntity?> GetOwnedFullAsync(long id, long userId, CancellationToken ct = default) =>
        await FullQuery().FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

    public async Task<bool> UpdateOwnedAsync(
        long id, long userId, MissionEntity source, CancellationToken ct = default)
    {
        var mission = await Context.Missions
            .Include(m => m.Uav)
            .Include(m => m.Algorithm)
            .Include(m => m.Objects)
            .Include(m => m.Results)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

        if (mission is null)
            return false;

        var routesInvalid = RoutesBecomeInvalid(mission, source);

        mission.Name = source.Name;
        mission.UavProfileId = source.UavProfileId;

        mission.Start.Latitude = source.Start.Latitude;
        mission.Start.Longitude = source.Start.Longitude;
        mission.Finish.Latitude = source.Finish.Latitude;
        mission.Finish.Longitude = source.Finish.Longitude;

        mission.Uav.AirspeedMs = source.Uav.AirspeedMs;
        mission.Uav.MaxFlightTimeMin = source.Uav.MaxFlightTimeMin;
        mission.Uav.ReservePercent = source.Uav.ReservePercent;
        mission.Uav.FlightAltitudeM = source.Uav.FlightAltitudeM;

        mission.Algorithm.Alpha = source.Algorithm.Alpha;
        mission.Algorithm.Beta = source.Algorithm.Beta;
        mission.Algorithm.EvaporationRate = source.Algorithm.EvaporationRate;
        mission.Algorithm.PheromoneDeposit = source.Algorithm.PheromoneDeposit;
        mission.Algorithm.AntsCount = source.Algorithm.AntsCount;
        mission.Algorithm.IterationsCount = source.Algorithm.IterationsCount;

        Context.RemoveRange(mission.Objects);
        mission.Objects = source.Objects.ToList();

        if (routesInvalid)
            Context.RemoveRange(mission.Results);

        await Context.SaveChangesAsync(ct);

        return true;
    }

    private static bool RoutesBecomeInvalid(MissionEntity current, MissionEntity updated)
    {
        if (current.Start.Latitude != updated.Start.Latitude || current.Start.Longitude != updated.Start.Longitude
            || current.Finish.Latitude != updated.Finish.Latitude || current.Finish.Longitude != updated.Finish.Longitude)
            return true;

        if (current.Uav.AirspeedMs != updated.Uav.AirspeedMs
            || current.Uav.MaxFlightTimeMin != updated.Uav.MaxFlightTimeMin
            || current.Uav.ReservePercent != updated.Uav.ReservePercent
            || current.Uav.FlightAltitudeM != updated.Uav.FlightAltitudeM)
            return true;

        var before = current.Objects.OrderBy(o => o.Number).Select(o => (o.Latitude, o.Longitude));
        var after = updated.Objects.OrderBy(o => o.Number).Select(o => (o.Latitude, o.Longitude));

        return !before.SequenceEqual(after);
    }

    private IQueryable<MissionEntity> FullQuery() =>
        Context.Missions.AsNoTracking()
            .Include(m => m.Uav)
            .Include(m => m.Algorithm)
            .Include(m => m.Objects.OrderBy(o => o.Number))
            .Include(m => m.Results)
                .ThenInclude(r => r.Legs.OrderBy(l => l.Order))
            .AsSplitQuery();
}
