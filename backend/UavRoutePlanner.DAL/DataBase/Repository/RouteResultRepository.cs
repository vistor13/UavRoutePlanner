using UavRoutePlanner.DAL.Entities;
using UavRoutePlanner.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UavRoutePlanner.DAL.DataBase.Repository;

public class RouteResultRepository(ApplicationDbContext context)
    : RepositoriesBase<RouteResultEntity>(context), IRouteResultRepository
{
    public async Task<List<RouteResultEntity>> GetByMissionIdAsync(long missionId, CancellationToken ct = default) =>
        await Context.RouteResults.AsNoTracking()
            .Include(r => r.Legs.OrderBy(l => l.Order))
            .Where(r => r.MissionId == missionId)
            .OrderBy(r => r.DepartureTimeUtc)
            .AsSplitQuery()
            .ToListAsync(ct);
}
